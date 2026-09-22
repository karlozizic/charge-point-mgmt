namespace CPMS.Simulator.Tests;

public class CommandLineTests
{
    [Fact]
    public void Defaults_are_usable_with_no_arguments()
    {
        var settings = CommandLine.Parse([]);

        Assert.Equal("ws://127.0.0.1:5000/OCPP", settings.Fleet.Charger.GatewayUrl.ToString());
        Assert.Equal(100, settings.Fleet.Chargers);
        Assert.Equal(20, settings.Fleet.ArrivalsPerSecond);
        Assert.False(settings.Seed);
    }

    [Fact]
    public void Every_option_is_read()
    {
        var settings = CommandLine.Parse(
        [
            "--url", "ws://gateway:9000/OCPP",
            "--api", "http://api:9001",
            "--chargers", "500",
            "--arrival", "50",
            "--session", "60",
            "--meter-interval", "5",
            "--idle", "10",
            "--duration", "600",
            "--id-prefix", "LOAD-",
            "--tag", "TAG-X",
            "--power", "22000",
            "--capacity", "75000",
            "--stop-grace", "9",
            "--seed"
        ]);

        Assert.Equal("ws://gateway:9000/OCPP", settings.Fleet.Charger.GatewayUrl.ToString());
        Assert.Equal("http://api:9001/", settings.ApiUrl.ToString());
        Assert.Equal(500, settings.Fleet.Chargers);
        Assert.Equal(50, settings.Fleet.ArrivalsPerSecond);
        Assert.Equal(TimeSpan.FromSeconds(60), settings.Fleet.Charger.SessionDuration);
        Assert.Equal(TimeSpan.FromSeconds(5), settings.Fleet.Charger.MeterInterval);
        Assert.Equal(TimeSpan.FromSeconds(10), settings.Fleet.Charger.IdleBetweenSessions);
        Assert.Equal(TimeSpan.FromSeconds(600), settings.Duration);
        Assert.Equal("LOAD-", settings.Fleet.IdPrefix);
        Assert.Equal("TAG-X", settings.Fleet.Charger.IdTag);
        Assert.Equal(22_000, settings.Fleet.Charger.PowerW);
        Assert.Equal(75_000, settings.Fleet.Charger.BatteryCapacityWh);
        Assert.Equal(TimeSpan.FromSeconds(9), settings.Fleet.Charger.StopGrace);
        Assert.Equal(1, settings.Fleet.Tags);
        Assert.True(settings.Seed);
    }

    [Fact]
    public void A_flag_given_a_value_is_still_a_flag_and_a_value_option_left_bare_is_refused()
    {
        Assert.True(CommandLine.Parse(["--seed"]).Seed);
        Assert.Throws<ArgumentException>(() => CommandLine.Parse(["--chargers"]));
    }

    [Theory]
    [InlineData("--chargers", "1OOO")]
    [InlineData("--arrival", "2O")]
    [InlineData("--duration", "6O")]
    [InlineData("--power", "eleven")]
    public void A_mistyped_number_is_refused_rather_than_silently_replaced(string option, string value)
    {
        Assert.Throws<ArgumentException>(() => CommandLine.Parse([option, value]));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("2.5")]
    public void A_charger_count_must_be_a_whole_number_of_at_least_one(string value)
    {
        Assert.Throws<ArgumentException>(() => CommandLine.Parse(["--chargers", value]));
    }

    [Fact]
    public void The_defaults_let_a_session_finish()
    {
        var settings = CommandLine.Parse([]);

        Assert.Empty(CommandLine.Warnings(settings));
        Assert.True(settings.Fleet.Charger.SessionDuration + settings.Fleet.Charger.IdleBetweenSessions
                    < settings.Duration);
    }

    [Fact]
    public void A_session_that_cannot_finish_is_warned_about()
    {
        var settings = CommandLine.Parse(["--session", "300", "--duration", "60"]);

        Assert.Contains(CommandLine.Warnings(settings), w => w.Contains("no session can finish"));
    }

    [Fact]
    public void A_ramp_that_eats_the_run_is_warned_about()
    {
        var settings = CommandLine.Parse(["--chargers", "1000", "--arrival", "5", "--duration", "120"]);

        Assert.Contains(CommandLine.Warnings(settings), w => w.Contains("ramp is"));
    }

    [Fact]
    public void Tags_are_spread_when_asked_for()
    {
        Assert.Single(CommandLine.Parse([]).Fleet.TagIds);
        Assert.Equal(4, CommandLine.Parse(["--tags", "4"]).Fleet.TagIds.Count);
    }

    [Fact]
    public void An_unknown_option_is_refused()
    {
        Assert.Throws<ArgumentException>(() => CommandLine.Parse(["--chargrs", "10"]));
    }

    [Fact]
    public void A_stray_argument_is_refused()
    {
        Assert.Throws<ArgumentException>(() => CommandLine.Parse(["500"]));
    }
}
