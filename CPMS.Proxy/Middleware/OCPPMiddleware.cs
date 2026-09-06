using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using CPMS.BuildingBlocks.Infrastructure.Logger;
using CPMS.Proxy.Controllers.OCPP_1._6;
using CPMS.Proxy.Models;
using CPMS.Proxy.OCPP_1._6;
using CPMS.Proxy.Services;

namespace CPMS.Proxy.Middleware;

/// <summary>
/// Accepts OCPP 1.6-J WebSocket connections on /OCPP/{chargePointId} and dispatches each CALL frame to
/// <see cref="ControllerOcpp16"/>. Every other path is passed down the pipeline.
/// </summary>
public class OcppMiddleware
{
    private const string ProtocolOcpp16 = "ocpp1.6";

    // [MessageType, "UniqueId", "Action", {payload}]  or  [MessageType, "UniqueId", {payload}]
    private static readonly Regex MessageRegExp = new(
        @"^\[\s*(\d+)\s*,\s*""([^""]+)""\s*,(?:\s*""(\w*)""\s*,)?\s*(.*)\s*\]$",
        RegexOptions.Compiled);

    private readonly RequestDelegate _next;
    private readonly ILoggerService _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    // Connected chargers live in this process only: the gateway cannot run as more than one instance yet.
    private readonly ConcurrentDictionary<string, ChargePointStatus> _chargePoints = new();

    public OcppMiddleware(RequestDelegate next, ILoggerService logger, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/OCPP", out var remaining))
        {
            await _next(context);
            return;
        }

        // The charge point id is the last path segment, so nested URLs like /OCPP/site1/CP-001 work.
        var chargePointId = remaining.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
        if (string.IsNullOrEmpty(chargePointId))
        {
            _logger.Error($"OCPPMiddleware => Invalid charge point path: {context.Request.Path}");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        if (!context.WebSockets.IsWebSocketRequest)
        {
            _logger.Error($"OCPPMiddleware => Non-WebSocket request for {chargePointId}");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        if (!context.WebSockets.WebSocketRequestedProtocols.Contains(ProtocolOcpp16))
        {
            _logger.Error($"OCPPMiddleware => Unsupported protocol: {string.Join(", ", context.WebSockets.WebSocketRequestedProtocols)}");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using var webSocket = await context.WebSockets.AcceptWebSocketAsync(ProtocolOcpp16);

        var chargePoint = new ChargePointStatus
        {
            Id = chargePointId,
            Protocol = ProtocolOcpp16,
            WebSocket = webSocket
        };

        // A reconnecting charger replaces its previous socket.
        if (_chargePoints.TryGetValue(chargePointId, out var previous))
            await CloseQuietly(previous, "New connection established");
        _chargePoints[chargePointId] = chargePoint;

        _logger.Info($"OCPPMiddleware => Charge point connected: {chargePointId}");

        try
        {
            await ReceiveLoop(chargePoint);
        }
        finally
        {
            // Only remove our own entry; a newer connection for the same id must survive.
            _chargePoints.TryRemove(new KeyValuePair<string, ChargePointStatus>(chargePointId, chargePoint));
            _logger.Info($"OCPPMiddleware => Charge point disconnected: {chargePointId}");
        }
    }

    private async Task ReceiveLoop(ChargePointStatus chargePoint)
    {
        var socket = chargePoint.WebSocket;
        var buffer = new byte[4096];
        var message = new MemoryStream();

        while (socket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;
            try
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            catch (Exception e)
            {
                _logger.Error($"OCPPMiddleware => Error receiving from {chargePoint.Id}: {e.Message}");
                return;
            }

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await CloseQuietly(chargePoint, string.Empty);
                return;
            }

            message.Write(buffer, 0, result.Count);
            if (!result.EndOfMessage)
                continue;

            var text = Encoding.UTF8.GetString(message.GetBuffer(), 0, (int)message.Length);
            message.SetLength(0);

            await ProcessMessage(text, chargePoint);
        }
    }

    private async Task ProcessMessage(string text, ChargePointStatus chargePoint)
    {
        _logger.Debug($"OCPPMiddleware => Received from {chargePoint.Id}: {text}");

        if (text == "ping")
            return; // some chargers send a text ping; nothing to answer

        var match = MessageRegExp.Match(text);
        if (!match.Success)
        {
            _logger.Error($"OCPPMiddleware => Invalid message format from {chargePoint.Id}: {text}");
            return;
        }

        var message = new OCPPMessage
        {
            MessageType = match.Groups[1].Value,
            UniqueId = match.Groups[2].Value,
            Action = match.Groups[3].Value,
            JsonPayload = match.Groups[4].Value
        };

        switch (message.MessageType)
        {
            case "2": // CALL from the charge point
                await HandleCall(message, chargePoint);
                break;

            case "3": // CALLRESULT
            case "4": // CALLERROR
                // The gateway sends no CALLs to chargers yet, so there is nothing to correlate this with.
                _logger.Warning($"OCPPMiddleware => Unexpected reply {message.UniqueId} from {chargePoint.Id}");
                break;

            default:
                _logger.Error($"OCPPMiddleware => Unknown message type {message.MessageType} from {chargePoint.Id}");
                break;
        }
    }

    private async Task HandleCall(OCPPMessage request, ChargePointStatus chargePoint)
    {
        OCPPMessage response;
        try
        {
            // Scoped so the typed HttpClient is resolved per message rather than rooted for the process lifetime.
            using var scope = _scopeFactory.CreateScope();
            var controller = new ControllerOcpp16(
                chargePoint,
                _logger,
                scope.ServiceProvider.GetRequiredService<ICpmsClient>(),
                scope.ServiceProvider.GetRequiredService<IAuthorizationCache>());

            response = await controller.ProcessRequest(request);
        }
        catch (Exception ex)
        {
            _logger.Error($"OCPPMiddleware => Error handling {request.Action} from {chargePoint.Id}: {ex.Message}");
            response = new OCPPMessage
            {
                MessageType = "4",
                UniqueId = request.UniqueId,
                ErrorCode = ErrorCodes.InternalError,
                ErrorDescription = "Internal error processing request"
            };
        }

        await Send(response, chargePoint);
    }

    private async Task Send(OCPPMessage message, ChargePointStatus chargePoint)
    {
        var text = message.MessageType switch
        {
            "3" => $"[3,\"{message.UniqueId}\",{message.JsonPayload}]",
            "4" => $"[4,\"{message.UniqueId}\",\"{message.ErrorCode}\",\"{message.ErrorDescription}\",{{}}]",
            _ => throw new InvalidOperationException($"Cannot send message type {message.MessageType}")
        };

        _logger.Debug($"OCPPMiddleware => Sending to {chargePoint.Id}: {text}");

        try
        {
            await chargePoint.WebSocket.SendAsync(
                Encoding.UTF8.GetBytes(text),
                WebSocketMessageType.Text,
                endOfMessage: true,
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            // The socket can die between receiving the CALL and answering it; the read loop ends on its own.
            _logger.Error($"OCPPMiddleware => Error sending to {chargePoint.Id}: {ex.Message}");
        }
    }

    /// <summary>
    /// Sends a close frame without waiting for the peer's answer. Covers both cases: acknowledging a
    /// close the charger started (state CloseReceived) and closing a socket we are replacing (state Open).
    /// </summary>
    private async Task CloseQuietly(ChargePointStatus chargePoint, string reason)
    {
        var state = chargePoint.WebSocket.State;
        if (state != WebSocketState.Open && state != WebSocketState.CloseReceived)
            return;

        try
        {
            await chargePoint.WebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, reason, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.Error($"OCPPMiddleware => Error closing socket for {chargePoint.Id}: {ex.Message}");
        }
    }
}

public static class OcppMiddlewareExtensions
{
    public static IApplicationBuilder UseOcppMiddleware(this IApplicationBuilder builder) =>
        builder.UseMiddleware<OcppMiddleware>();
}
