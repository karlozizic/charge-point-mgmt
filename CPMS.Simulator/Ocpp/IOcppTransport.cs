namespace CPMS.Simulator.Ocpp;

// The seam that keeps OcppConnection testable without a socket.
public interface IOcppTransport : IAsyncDisposable
{
    Task SendAsync(string frame, CancellationToken ct);

    IAsyncEnumerable<string> ReceiveAsync(CancellationToken ct);
}
