using System.Diagnostics;
using CPMS.Simulator.Ocpp;

namespace CPMS.Simulator.Tests;

public class FleetTests
{
    private static FleetOptions Options(int chargers, double arrivalsPerSecond, int tags = 1) => new(
        new ChargerOptions(
            GatewayUrl: new Uri("ws://localhost:5000/OCPP"),
            IdTag: "TAG-SIM",
            SessionDuration: TimeSpan.FromMilliseconds(200),
            MeterInterval: TimeSpan.FromMilliseconds(100),
            IdleBetweenSessions: TimeSpan.FromMilliseconds(50),
            StopGrace: TimeSpan.FromMilliseconds(500),
            BatteryCapacityWh: 50_000,
            PowerW: 11_000),
        Chargers: chargers,
        ArrivalsPerSecond: arrivalsPerSecond,
        IdPrefix: "CP-SIM-",
        Tags: tags);

    // Runs the fleet and returns once every charger has connected, so a test never waits out its
    // own cancellation timeout. Elapsed is measured from the first arrival to the last, inside the
    // connect callback. The fleet connects its first charger synchronously, and that iteration also
    // pays for JIT of the whole charger stack, so a clock started around the call either misses the
    // first arrival or charges the JIT to the pacing - both of which move the number by more than
    // the interval being measured.
    private static async Task<(List<Uri> Urls, TimeSpan Elapsed, Task Run)> ConnectAll(FleetOptions options)
    {
        var urls = new List<Uri>();
        var all = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cts = new CancellationTokenSource();
        var first = 0L;
        var last = 0L;

        var run = new Fleet(options, new Metrics(), (url, _) =>
        {
            lock (urls)
            {
                if (urls.Count == 0) first = Stopwatch.GetTimestamp();
                last = Stopwatch.GetTimestamp();
                urls.Add(url);
                if (urls.Count == options.Chargers) all.SetResult();
            }
            return Task.FromResult<IOcppTransport>(new FakeTransport());
        }).RunAsync(cts.Token);

        await all.Task.WaitAsync(TimeSpan.FromSeconds(10));
        var elapsed = Stopwatch.GetElapsedTime(first, last);

        await cts.CancelAsync();
        return (urls, elapsed, run);
    }

    [Fact]
    public void Charger_ids_are_numbered_from_the_prefix()
    {
        Assert.Equal(["CP-SIM-0001", "CP-SIM-0002", "CP-SIM-0003"], Options(3, 100).ChargerIds.ToArray());
    }

    [Fact]
    public void One_tag_is_used_as_given_and_many_are_numbered()
    {
        Assert.Equal(["TAG-SIM"], Options(5, 100).TagIds);
        Assert.Equal(["TAG-SIM-0001", "TAG-SIM-0002", "TAG-SIM-0003"], Options(5, 100, tags: 3).TagIds);
    }

    [Fact]
    public async Task Every_charger_gets_its_own_connection()
    {
        var (urls, _, run) = await ConnectAll(Options(5, 1000));

        Assert.Equal(5, urls.Select(u => u.ToString()).Distinct().Count());
        Assert.Contains(urls, u => u.ToString().EndsWith("/CP-SIM-0005"));

        await run.WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task Arrivals_are_paced()
    {
        var (_, elapsed, run) = await ConnectAll(Options(5, arrivalsPerSecond: 10));

        // Four gaps of 100 ms between the first arrival and the fifth.
        Assert.True(elapsed >= TimeSpan.FromMilliseconds(350),
            $"5 chargers at 10/s should spread over about 400 ms, took {elapsed.TotalMilliseconds:F0} ms");
        Assert.True(elapsed < TimeSpan.FromMilliseconds(1500),
            $"pacing should not drift; took {elapsed.TotalMilliseconds:F0} ms");

        await run.WaitAsync(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task Cancelling_stops_every_charger()
    {
        var (_, _, run) = await ConnectAll(Options(10, 1000));

        await run.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.True(run.IsCompleted);
    }
}
