using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using CPMS.BuildingBlocks.Infrastructure.Logger;
using CPMS.Proxy.Controllers.OCPP_1._6;
using CPMS.Proxy.Models;
using CPMS.Proxy.OCPP_1._6;
using CPMS.Proxy.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ControllerOcpp16 = CPMS.Proxy.Controllers.OCPP_1._6.ControllerOcpp16;

namespace CPMS.Proxy.Configuration;

public partial class OcppMiddleware
{
    private const string ProtocolOcpp16 = "ocpp1.6";
    private static readonly string[] SupportedProtocols = { ProtocolOcpp16 };
    private static readonly Regex MessageRegExp = new Regex(
        @"^\[\s*(\d+)\s*,\s*""([^""]+)""\s*,(?:\s*""(\w*)""\s*,)?\s*(.*)\s*\]$", 
        RegexOptions.Compiled);

    private readonly IConfiguration _configuration;
    private readonly RequestDelegate _next;
    private readonly ILoggerService _logger;
    private readonly IServiceProvider _serviceProvider;
    
    private readonly ConcurrentDictionary<string, ChargePointStatus> _chargePoints = new();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<OCPPMessage>> _pendingRequests = new();
    
    public OcppMiddleware(
        IConfiguration config,
        RequestDelegate next,
        ILoggerService logger,
        IServiceProvider serviceProvider)
    {
        _configuration = config;
        _next = next;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/OCPP"))
        {
            await HandleOcppConnection(context);
        }
        else if (context.Request.Path.StartsWithSegments("/api"))
        {
            //todo: stream?
            await _next(context);
        }
        else
        {
            _logger.Info($"OCPPMiddleware => request with invalid path {context.Request.Path}");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        } 
    }

    private async Task HandleOcppConnection(HttpContext context)
    {
        string? chargePointId = ExtractChargePointId(context.Request.Path);
        if (string.IsNullOrEmpty(chargePointId))
        {
            _logger.Error("OCPPMiddleware => Invalid chargepoint ID");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }
        
        if (!context.WebSockets.IsWebSocketRequest)
        {
            _logger.Error("OCPPMiddleware => Non-WebSocket request");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }
        
        string? protocol = null;
        foreach (string supportedProtocol in SupportedProtocols)
        {
            if (context.WebSockets.WebSocketRequestedProtocols.Contains(supportedProtocol))
            {
                protocol = supportedProtocol;
                break;
            }
        }
        
        if (string.IsNullOrEmpty(protocol))
        {
            _logger.Error($"OCPPMiddleware => Unsupported protocol: {string.Join(", ", context.WebSockets.WebSocketRequestedProtocols)}");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }
        
        using WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync(protocol);
        
        var chargePointStatus = new ChargePointStatus
        {
            Id = chargePointId,
            WebSocket = webSocket,
            Protocol = protocol
        };
        
        // Add to connected charge points
        if (!_chargePoints.TryAdd(chargePointId, chargePointStatus))
        {
            // Remove existing connection if present
            if (_chargePoints.TryGetValue(chargePointId, out var existingStatus))
            {
                if (existingStatus.WebSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        await existingStatus.WebSocket.CloseAsync(
                            WebSocketCloseStatus.PolicyViolation,
                            "New connection established",
                            CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"OCPPMiddleware => Error closing existing connection: {ex.Message}");
                    }
                }
                
                _chargePoints.TryRemove(chargePointId, out _);
                _chargePoints.TryAdd(chargePointId, chargePointStatus);
            }
        }
        
        _logger.Info($"OCPPMiddleware => Charge point connected: {chargePointId}");
        
        await HandleWebSocketConnection(chargePointStatus);
    }
    
    private async Task HandleWebSocketConnection(ChargePointStatus chargePointStatus)
    {
        var buffer = new byte[4096];
        var memStream = new MemoryStream();
        
        try
        {
            while (chargePointStatus.WebSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                try
                {
                    result = await chargePointStatus.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer),
                        CancellationToken.None);
                }
                catch (Exception e)
                {
                    _logger.Error($"OCPPMiddleware => Error receiving WebSocket message: {e.Message}");
                    break;
                }
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await chargePointStatus.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    break;
                }
                
                memStream.Write(buffer, 0, result.Count);
                
                if (result.EndOfMessage)
                {
                    var messageBytes = memStream.ToArray();
                    memStream = new MemoryStream();
                    
                    var message = Encoding.UTF8.GetString(messageBytes);
                    await ProcessOcppMessage(message, chargePointStatus);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"OCPPMiddleware => Error handling WebSocket: {ex.Message}");
        }
        finally
        {
            _chargePoints.TryRemove(chargePointStatus.Id, out _);
            _logger.Info($"OCPPMiddleware => Charge point disconnected: {chargePointStatus.Id}");
        }
    }
    
    private async Task ProcessOcppMessage(string message, ChargePointStatus chargePointStatus)
    {
        try
        {
            // Log incoming message
            _logger.Debug($"OCPPMiddleware => Received message: {message}");
            if (message == "ping")
            {
                return;
            }
            // Parse message
            var match = MessageRegExp.Match(message);
            if (!match.Success)
            {
                _logger.Error($"OCPPMiddleware => Invalid message format: {message}");
                return;
            }
            
            var messageType = match.Groups[1].Value;
            var uniqueId = match.Groups[2].Value;
            var action = match.Groups[3].Value;
            var jsonPayload = match.Groups[4].Value;
            
            _logger.Info($"OCPPMiddleware => Message: Type={messageType}, ID={uniqueId}, Action={action}");
            
            var ocppMessage = new OCPPMessage
            {
                MessageType = messageType,
                UniqueId = uniqueId,
                Action = action,
                JsonPayload = jsonPayload
            };
            
            // Process message based on type
            switch (messageType)
            {
                case "2": // Request from charge point
                    await HandleChargePointRequest(ocppMessage, chargePointStatus);
                    break;
                    
                case "3": // Response from charge point
                case "4": // Error from charge point
                    HandleChargePointResponse(ocppMessage);
                    break;
                    
                default:
                    _logger.Error($"OCPPMiddleware => Unknown message type: {messageType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"OCPPMiddleware => Error processing message: {ex.Message}");
        }
    }
    
    private async Task HandleChargePointRequest(OCPPMessage request, ChargePointStatus chargePointStatus)
    {
        try
        {
            var controller = CreateController(chargePointStatus);
            
            var response = await controller.ProcessRequest(request);
            
            await SendOcppMessage(response, chargePointStatus);
        }
        catch (Exception ex)
        {
            _logger.Error($"OCPPMiddleware => Error handling request: {ex.Message}");
            
            var errorResponse = new OCPPMessage
            {
                MessageType = "4",
                UniqueId = request.UniqueId,
                ErrorCode = ErrorCodes.InternalError,
                ErrorDescription = "Internal error processing request"
            };
            
            await SendOcppMessage(errorResponse, chargePointStatus);
        }
    }
    
    private void HandleChargePointResponse(OCPPMessage response)
    {
        if (_pendingRequests.TryRemove(response.UniqueId, out var taskCompletionSource))
        {
            taskCompletionSource.SetResult(response);
        }
        else
        {
            _logger.Warning($"OCPPMiddleware => Received response with no matching request: {response.UniqueId}");
        }
    }
    
    private ControllerOcpp16 CreateController(ChargePointStatus chargePointStatus)
    {
        var cpmsClient = _serviceProvider.GetRequiredService<ICpmsClient>();
        var authorizationCache = _serviceProvider.GetRequiredService<IAuthorizationCache>();
        
        return new ControllerOcpp16(
            _configuration,
            chargePointStatus,
            _logger,
            cpmsClient,
            authorizationCache);
    }
    
    private async Task SendOcppMessage(OCPPMessage message, ChargePointStatus chargePointStatus)
    {
        string messageText;
        
        switch (message.MessageType)
        {
            case "2": // Request
                messageText = $"[{message.MessageType},\"{message.UniqueId}\",\"{message.Action}\",{message.JsonPayload}]";
                break;
                
            case "3": // Response
                messageText = $"[{message.MessageType},\"{message.UniqueId}\",{message.JsonPayload}]";
                break;
                
            case "4": // Error
                messageText = $"[{message.MessageType},\"{message.UniqueId}\",\"{message.ErrorCode}\",\"{message.ErrorDescription}\",{{}}]";
                break;
                
            default:
                _logger.Error($"OCPPMiddleware => Invalid message type: {message.MessageType}");
                return;
        }
        
        _logger.Debug($"OCPPMiddleware => Sending message: {messageText}");
        
        var bytes = Encoding.UTF8.GetBytes(messageText);
        await chargePointStatus.WebSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }
    
    private string? ExtractChargePointId(PathString path)
    {
        string?[] parts = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? [];
        return parts.Length > 1 ? parts[parts.Length - 1] : null;
    }
    
}

public static class OcppMiddlewareExtensions
{
    public static IApplicationBuilder UseOCPPMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<OcppMiddleware>();
    }
}