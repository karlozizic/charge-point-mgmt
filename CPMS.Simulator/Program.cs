using CPMS.Simulator;

RunSettings settings;
try
{
    settings = CommandLine.Parse(args);
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    Console.Error.WriteLine();
    Console.Error.WriteLine(CommandLine.Usage);
    return 1;
}

foreach (var warning in CommandLine.Warnings(settings)) Console.WriteLine($"warning: {warning}");

using var stopping = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    try
    {
        stopping.Cancel();
    }
    catch (ObjectDisposedException)
    {
        // A second Ctrl+C after the run has already returned; there is nothing left to stop.
    }
};

if (settings.Seed)
{
    using var http = new HttpClient { BaseAddress = settings.ApiUrl };
    SeedResult seeded;
    try
    {
        seeded = await new Seeder(http).EnsureAsync(
            settings.Fleet.ChargerIds, settings.Fleet.TagIds, stopping.Token);
    }
    catch (OperationCanceledException) when (stopping.IsCancellationRequested)
    {
        Console.WriteLine("stopped while seeding; run again with --seed to finish");
        return 130;
    }

    Console.WriteLine($"seeded: {seeded.ChargePointsCreated} charge points, {seeded.TagsCreated} tags, " +
                      $"{seeded.PricingAssigned} pricing assignments");
    Console.WriteLine($"        hand-driven charger ready: {Seeder.ManualChargerId} with {Seeder.ManualTagId}");
}

var metrics = new Metrics(settings.Fleet.Chargers);
var fleet = new Fleet(settings.Fleet, metrics);

Console.WriteLine($"{settings.Fleet.Chargers} chargers -> {settings.Fleet.Charger.GatewayUrl}, " +
                  $"{settings.Fleet.ArrivalsPerSecond}/s arrival, {settings.Fleet.TagIds.Count} tag(s), " +
                  $"{settings.Duration.TotalSeconds:F0} s run");

stopping.CancelAfter(settings.Duration);
await fleet.RunAsync(stopping.Token);

Console.WriteLine();
Console.WriteLine($"ramp took {fleet.RampDuration.TotalSeconds:F1} s");
Console.WriteLine(metrics.Snapshot());
return 0;
