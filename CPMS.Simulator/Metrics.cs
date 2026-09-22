using System.Collections.Concurrent;
using System.Diagnostics;

namespace CPMS.Simulator;

// A CALLRESULT that says "no" is not a success. Counting it as one makes an unseeded run - where
// the gateway rejects every BootNotification - read as a clean, fast one.
public enum Outcome
{
    Ok,
    Rejected,
    Failed
}

public sealed record ActionReport(
    string Action, long Ok, long Rejected, long Failed, double P50Ms, double P95Ms, double P99Ms);

public sealed record Report(
    TimeSpan Elapsed,
    long Messages,
    double MessagesPerSecond,
    int PeakConnected,
    int Chargers,
    IReadOnlyList<ActionReport> Actions)
{
    public override string ToString()
    {
        var connected = Chargers > 0
            ? $", peak {PeakConnected} of {Chargers} connected"
            : $", peak {PeakConnected} connected";

        var lines = new List<string>
        {
            $"ran for {Elapsed.TotalSeconds:F1} s, {Messages} messages, {MessagesPerSecond:F1} msg/s{connected}",
            $"{"action",-20}{"ok",8}{"rejected",10}{"failed",8}{"p50 ms",10}{"p95 ms",10}{"p99 ms",10}"
        };

        lines.AddRange(Actions.Select(a =>
            $"{a.Action,-20}{a.Ok,8}{a.Rejected,10}{a.Failed,8}{a.P50Ms,10:F1}{a.P95Ms,10:F1}{a.P99Ms,10:F1}"));

        if (PeakConnected < Chargers)
        {
            lines.Add($"WARNING: {Chargers - PeakConnected} chargers never connected at the same time.");
        }

        return string.Join(Environment.NewLine, lines);
    }
}

public sealed class Metrics(int chargers = 0)
{
    public const string ConnectAction = "Connect";

    private readonly ConcurrentDictionary<string, Samples> _byAction = new();
    private readonly long _start = Stopwatch.GetTimestamp();
    private int _connected;
    private int _peakConnected;

    public void Record(string action, TimeSpan latency, Outcome outcome)
        => _byAction.GetOrAdd(action, _ => new Samples()).Add(latency.TotalMilliseconds, outcome);

    public void Connected()
    {
        var now = Interlocked.Increment(ref _connected);
        var peak = Volatile.Read(ref _peakConnected);
        while (now > peak && Interlocked.CompareExchange(ref _peakConnected, now, peak) != peak)
        {
            peak = Volatile.Read(ref _peakConnected);
        }
    }

    public void Disconnected() => Interlocked.Decrement(ref _connected);

    public Report Snapshot()
    {
        var actions = _byAction
            .OrderBy(pair => pair.Key)
            .Select(pair => pair.Value.Report(pair.Key))
            .ToList();

        // Connect attempts are in the table for their latency, but they are not OCPP messages. A
        // flapping gateway would otherwise inflate msg/s and make a bad run read as a busy one.
        var elapsed = Stopwatch.GetElapsedTime(_start);
        var messages = actions.Where(a => a.Action != ConnectAction).Sum(a => a.Ok + a.Rejected + a.Failed);
        var perSecond = elapsed.TotalSeconds > 0 ? messages / elapsed.TotalSeconds : 0;

        return new Report(elapsed, messages, perSecond, Volatile.Read(ref _peakConnected), chargers, actions);
    }

    private sealed class Samples
    {
        private readonly List<double> _latencies = [];
        private long _ok;
        private long _rejected;
        private long _failed;

        public void Add(double ms, Outcome outcome)
        {
            lock (_latencies)
            {
                _latencies.Add(ms);
                switch (outcome)
                {
                    case Outcome.Ok: _ok++; break;
                    case Outcome.Rejected: _rejected++; break;
                    default: _failed++; break;
                }
            }
        }

        public ActionReport Report(string action)
        {
            lock (_latencies)
            {
                var sorted = _latencies.Order().ToArray();
                return new ActionReport(action, _ok, _rejected, _failed,
                    Percentile(sorted, 0.50), Percentile(sorted, 0.95), Percentile(sorted, 0.99));
            }
        }

        private static double Percentile(double[] sorted, double fraction)
        {
            if (sorted.Length == 0) return 0;
            var index = (int)Math.Ceiling(fraction * sorted.Length) - 1;
            return sorted[Math.Clamp(index, 0, sorted.Length - 1)];
        }
    }
}
