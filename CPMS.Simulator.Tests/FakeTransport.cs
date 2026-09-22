using System.Threading.Channels;
using CPMS.Simulator.Ocpp;

namespace CPMS.Simulator.Tests;

// An in-memory IOcppTransport. Both directions are channels, so a reader awaits rather than polls
// and ends by itself when the charger disposes the transport.
public sealed class FakeTransport : IOcppTransport
{
    private readonly Channel<string> _inbound = Channel.CreateUnbounded<string>();
    private readonly Channel<string> _outbound = Channel.CreateUnbounded<string>();

    public List<string> Sent { get; } = [];

    public IAsyncEnumerable<string> SentFrames => _outbound.Reader.ReadAllAsync();

    public Task SendAsync(string frame, CancellationToken ct)
    {
        lock (Sent) Sent.Add(frame);
        _outbound.Writer.TryWrite(frame);
        return Task.CompletedTask;
    }

    public IAsyncEnumerable<string> ReceiveAsync(CancellationToken ct) => _inbound.Reader.ReadAllAsync(ct);

    public void Deliver(string frame) => _inbound.Writer.TryWrite(frame);

    public void Close()
    {
        _inbound.Writer.TryComplete();
        _outbound.Writer.TryComplete();
    }

    public ValueTask DisposeAsync()
    {
        Close();
        return ValueTask.CompletedTask;
    }

    public async Task<string> WaitForSentAsync(int count, TimeSpan? within = null)
    {
        var deadline = DateTime.UtcNow + (within ?? TimeSpan.FromSeconds(2));
        while (DateTime.UtcNow < deadline)
        {
            lock (Sent)
            {
                if (Sent.Count >= count) return Sent[count - 1];
            }
            await Task.Delay(5);
        }

        lock (Sent) throw new TimeoutException($"Only {Sent.Count} frames were sent, expected {count}");
    }
}
