using System.Net.WebSockets;
using System.Text;

namespace CPMS.Simulator.Ocpp;

public sealed class WebSocketTransport(ClientWebSocket socket) : IOcppTransport
{
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    public static async Task<IOcppTransport> ConnectAsync(Uri url, CancellationToken ct)
    {
        var socket = new ClientWebSocket();
        socket.Options.AddSubProtocol("ocpp1.6");
        await socket.ConnectAsync(url, ct);
        return new WebSocketTransport(socket);
    }

    // ClientWebSocket allows one send at a time; the heartbeat and the meter timer both write.
    public async Task SendAsync(string frame, CancellationToken ct)
    {
        await _sendLock.WaitAsync(ct);
        try
        {
            await socket.SendAsync(Encoding.UTF8.GetBytes(frame), WebSocketMessageType.Text, true, ct);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public async IAsyncEnumerable<string> ReceiveAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        var buffer = new byte[8192];
        var message = new MemoryStream();

        // Cancelling ClientWebSocket.ReceiveAsync aborts the socket, and the gateway then logs a
        // half-closed connection. Ask for a clean close instead and let the read loop end on its own.
        using var onCancel = ct.Register(() => _ = CloseOutputAsync());

        while (socket.State is WebSocketState.Open or WebSocketState.CloseSent)
        {
            WebSocketReceiveResult result;
            try
            {
                result = await socket.ReceiveAsync(buffer, CancellationToken.None);
            }
            catch (Exception exception)
                when (exception is WebSocketException or OperationCanceledException or ObjectDisposedException)
            {
                yield break;
            }

            if (result.MessageType == WebSocketMessageType.Close) yield break;

            // OCPP frames fit in the buffer, so the common case needs no copy at all.
            if (result.EndOfMessage && message.Length == 0)
            {
                yield return Encoding.UTF8.GetString(buffer, 0, result.Count);
                continue;
            }

            message.Write(buffer, 0, result.Count);
            if (!result.EndOfMessage) continue;

            var text = Encoding.UTF8.GetString(message.GetBuffer(), 0, (int)message.Length);
            message.SetLength(0);
            yield return text;
        }
    }

    private async Task CloseOutputAsync()
    {
        try
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            }
        }
        catch (Exception exception) when (exception is WebSocketException or ObjectDisposedException)
        {
            // The peer is already gone; nothing left to close politely.
        }
    }

    public async ValueTask DisposeAsync()
    {
        await CloseOutputAsync();
        socket.Dispose();
        _sendLock.Dispose();
    }
}
