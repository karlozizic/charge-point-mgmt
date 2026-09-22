using System.Diagnostics;
using CPMS.Simulator.Ocpp;

namespace CPMS.Simulator;

public sealed record FleetOptions(
    ChargerOptions Charger,
    int Chargers,
    double ArrivalsPerSecond,
    string IdPrefix,
    int Tags)
{
    public IEnumerable<string> ChargerIds => Enumerable.Range(1, Chargers).Select(n => $"{IdPrefix}{n:D4}");

    // One tag for the whole fleet means one ChargeTag aggregate stream taking every authorize in the
    // run. That measures a hot key, not a fleet.
    public IReadOnlyList<string> TagIds => Tags <= 1
        ? [Charger.IdTag]
        : Enumerable.Range(1, Tags).Select(n => $"{Charger.IdTag}-{n:D4}").ToArray();
}

public sealed class Fleet(
    FleetOptions options,
    Metrics metrics,
    Func<Uri, CancellationToken, Task<IOcppTransport>>? connect = null)
{
    private readonly Func<Uri, CancellationToken, Task<IOcppTransport>> _connect =
        connect ?? WebSocketTransport.ConnectAsync;

    public TimeSpan RampDuration { get; private set; }

    // Chargers arrive at a fixed rate. Opening every socket at once measures a connection storm,
    // not steady-state throughput. Pacing is against the clock the loop started on, so a slow
    // charger does not push every later arrival further behind.
    public async Task RunAsync(CancellationToken ct)
    {
        var running = new List<Task>(options.Chargers);
        var soc = new Random(1);
        var tags = options.TagIds;
        var start = Stopwatch.GetTimestamp();
        var started = 0;

        foreach (var id in options.ChargerIds)
        {
            if (ct.IsCancellationRequested) break;

            var charger = options.Charger with { IdTag = tags[started % tags.Count] };
            running.Add(new ChargerClient(id, soc.Next(10, 60), charger, _connect, metrics).RunAsync(ct));
            started++;

            if (options.ArrivalsPerSecond <= 0) continue;

            var due = TimeSpan.FromSeconds(started / options.ArrivalsPerSecond);
            var behind = due - Stopwatch.GetElapsedTime(start);
            if (behind > TimeSpan.Zero) await Task.Delay(behind, CancellationToken.None);
        }

        RampDuration = Stopwatch.GetElapsedTime(start);
        await Task.WhenAll(running);
    }
}
