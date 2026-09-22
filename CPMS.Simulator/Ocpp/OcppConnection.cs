using System.Collections.Concurrent;
using System.Text.Json;

namespace CPMS.Simulator.Ocpp;

public sealed class OcppCallErrorException(string errorCode, string description)
    : Exception($"{errorCode}: {description}")
{
    public string ErrorCode { get; } = errorCode;
}

// Sends CALLs and matches each answer back to the caller by uniqueId. Every outstanding call has a
// bounded timeout, so a dropped answer fails one caller instead of leaking a task forever.
public sealed class OcppConnection(IOcppTransport transport, TimeSpan? callTimeout = null)
{
    private readonly ConcurrentDictionary<string, TaskCompletionSource<OcppFrame>> _pending = new();
    private readonly TimeSpan _callTimeout = callTimeout ?? TimeSpan.FromSeconds(30);

    // Answers a CSMS-initiated CALL. Return null to refuse with NotImplemented.
    public Func<string, JsonElement, object?>? OnCall { get; set; }

    public async Task<JsonElement> CallAsync(string action, object payload, CancellationToken ct)
    {
        var uniqueId = Guid.NewGuid().ToString("N");
        var waiter = new TaskCompletionSource<OcppFrame>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending[uniqueId] = waiter;

        try
        {
            var body = JsonSerializer.SerializeToElement(payload);
            await transport.SendAsync(OcppFrame.Call(uniqueId, action, body).Serialize(), ct);

            var answer = await waiter.Task.WaitAsync(_callTimeout, ct);
            return answer.Type == OcppMessageType.CallError
                ? throw new OcppCallErrorException(answer.ErrorCode!, answer.ErrorDescription!)
                : answer.Payload;
        }
        finally
        {
            _pending.TryRemove(uniqueId, out _);
        }
    }

    // Reads until the transport ends or the token trips. Faults every waiting call on the way out,
    // so a closed socket never leaves a caller hanging.
    public async Task RunAsync(CancellationToken ct)
    {
        try
        {
            await foreach (var text in transport.ReceiveAsync(ct))
            {
                if (!OcppFrame.TryParse(text, out var frame)) continue;

                if (frame.Type == OcppMessageType.Call)
                {
                    await AnswerAsync(frame, ct);
                }
                else if (_pending.TryRemove(frame.UniqueId, out var waiter))
                {
                    waiter.TrySetResult(frame);
                }
            }
        }
        finally
        {
            foreach (var waiter in _pending.Values)
            {
                waiter.TrySetException(new IOException("The connection closed before the answer arrived"));
            }
            _pending.Clear();
        }
    }

    private async Task AnswerAsync(OcppFrame call, CancellationToken ct)
    {
        object? answer;
        try
        {
            answer = OnCall?.Invoke(call.Action!, call.Payload);
        }
        catch (Exception exception)
        {
            await transport.SendAsync(
                OcppFrame.Error(call.UniqueId, "InternalError", exception.Message).Serialize(), ct);
            return;
        }

        var reply = answer is null
            ? OcppFrame.Error(call.UniqueId, "NotImplemented", $"Unknown action {call.Action}")
            : OcppFrame.Result(call.UniqueId, JsonSerializer.SerializeToElement(answer));

        await transport.SendAsync(reply.Serialize(), ct);
    }
}
