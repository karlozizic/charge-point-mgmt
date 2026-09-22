using System.Globalization;

namespace CPMS.Simulator;

public sealed record RunSettings(FleetOptions Fleet, Uri ApiUrl, bool Seed, TimeSpan Duration);

public static class CommandLine
{
    public const string Usage = """
        Usage: dotnet run --project CPMS.Simulator -- [options]

          --url <ws://host:port/OCPP>   gateway
          --api <http://host:port>      API, used by --seed
          --chargers <n>                how many
          --arrival <n>                 chargers per second
          --session <seconds>           session length
          --meter-interval <seconds>    seconds between MeterValues
          --idle <seconds>              gap between sessions
          --stop-grace <seconds>        time a running transaction gets to close at shutdown
          --duration <seconds>          how long to run
          --id-prefix <text>            charger ids, numbered from 0001
          --tag <text>                  id tag
          --tags <n>                    spread the fleet over n tags instead of one
          --power <watts>               charging power
          --capacity <wh>               battery size
          --seed                        create the charge points, the tags and a pricing group first

        Defaults are listed in CPMS.Simulator/README.md.
        """;

    // Options are consumed as they are read, so whatever is left over at the end is a typo. That
    // removes the need for a separate list of known names to drift out of step.
    public static RunSettings Parse(string[] args)
    {
        var given = new Dictionary<string, string?>();

        for (var i = 0; i < args.Length; i++)
        {
            if (!args[i].StartsWith("--")) throw new ArgumentException($"Unexpected argument {args[i]}");

            var name = args[i][2..];
            given[name] = i + 1 < args.Length && !args[i + 1].StartsWith("--") ? args[++i] : null;
        }

        string? Text(string name)
        {
            if (!given.Remove(name, out var value)) return null;
            return value ?? throw new ArgumentException($"--{name} needs a value");
        }

        bool Flag(string name) => given.Remove(name, out _);

        // A value that is present but unparsable is a typo, not a request for the default:
        // --chargers 1OOO would otherwise run 100 and say nothing.
        double Number(string name, double fallback)
        {
            var text = Text(name);
            if (text is null) return fallback;
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                ? value
                : throw new ArgumentException($"--{name} needs a number, got '{text}'");
        }

        int Count(string name, int fallback)
        {
            var value = Number(name, fallback);
            return value >= 1 && value == Math.Floor(value)
                ? (int)value
                : throw new ArgumentException($"--{name} needs a whole number of at least 1, got '{value}'");
        }

        TimeSpan Seconds(string name, double fallback) => TimeSpan.FromSeconds(Number(name, fallback));

        var charger = new ChargerOptions(
            GatewayUrl: new Uri(Text("url") ?? "ws://127.0.0.1:5000/OCPP"),
            IdTag: Text("tag") ?? "TAG-SIM",
            SessionDuration: Seconds("session", 30),
            MeterInterval: Seconds("meter-interval", 15),
            IdleBetweenSessions: Seconds("idle", 10),
            StopGrace: Seconds("stop-grace", 5),
            BatteryCapacityWh: Number("capacity", 50_000),
            PowerW: Number("power", 11_000));

        var settings = new RunSettings(
            new FleetOptions(charger,
                Chargers: Count("chargers", 100),
                ArrivalsPerSecond: Number("arrival", 20),
                IdPrefix: Text("id-prefix") ?? "CP-SIM-",
                Tags: Count("tags", 1)),
            ApiUrl: new Uri(Text("api") ?? "http://localhost:5023"),
            Seed: Flag("seed"),
            Duration: Seconds("duration", 120));

        if (given.Count > 0) throw new ArgumentException($"Unknown option --{given.Keys.First()}");
        return settings;
    }

    // Not fatal: a ramp-only or boot-only run is a legitimate thing to ask for. But a run whose
    // sessions cannot finish measures a shutdown stampede, not a lifecycle.
    public static IEnumerable<string> Warnings(RunSettings settings)
    {
        var charger = settings.Fleet.Charger;
        var ramp = settings.Fleet.ArrivalsPerSecond > 0
            ? TimeSpan.FromSeconds(settings.Fleet.Chargers / settings.Fleet.ArrivalsPerSecond)
            : TimeSpan.Zero;

        if (ramp + charger.SessionDuration > settings.Duration)
        {
            yield return $"no session can finish: ramp {ramp.TotalSeconds:F0} s + session " +
                         $"{charger.SessionDuration.TotalSeconds:F0} s exceeds --duration " +
                         $"{settings.Duration.TotalSeconds:F0} s.";
        }

        if (ramp > settings.Duration / 2)
        {
            yield return $"the ramp is {ramp.TotalSeconds:F0} s of a {settings.Duration.TotalSeconds:F0} s run; " +
                         "raise --arrival or --duration.";
        }

        if (charger.MeterInterval > charger.SessionDuration)
        {
            yield return "only one MeterValues per session, at its end: --meter-interval is longer than --session.";
        }
    }
}
