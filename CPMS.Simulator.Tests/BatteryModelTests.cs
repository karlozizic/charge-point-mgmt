namespace CPMS.Simulator.Tests;

public class BatteryModelTests
{
    private const double CapacityWh = 50_000;

    private static BatteryModel Battery(double socPercent = 0, double powerW = 11_000)
        => new(CapacityWh, socPercent, powerW);

    [Fact]
    public void A_new_battery_has_delivered_nothing()
    {
        Assert.Equal(0, Battery().EnergyWh);
    }

    [Fact]
    public void A_new_battery_reports_the_state_of_charge_it_was_given()
    {
        Assert.Equal(23, Battery(socPercent: 23).SoCPercent, 6);
    }

    [Fact]
    public void An_hour_at_eleven_kilowatts_delivers_eleven_kilowatt_hours()
    {
        var battery = Battery(powerW: 11_000);

        battery.Advance(TimeSpan.FromHours(1));

        Assert.Equal(11_000, battery.EnergyWh, 6);
    }

    [Fact]
    public void Energy_accumulates_across_calls()
    {
        var battery = Battery(powerW: 11_000);

        battery.Advance(TimeSpan.FromMinutes(30));
        battery.Advance(TimeSpan.FromMinutes(30));

        Assert.Equal(11_000, battery.EnergyWh, 6);
    }

    [Fact]
    public void The_state_of_charge_follows_the_energy()
    {
        var battery = Battery(socPercent: 0, powerW: 11_000);

        battery.Advance(TimeSpan.FromHours(1));

        Assert.Equal(22, battery.SoCPercent, 6);
    }

    [Fact]
    public void The_state_of_charge_starts_from_where_the_battery_was()
    {
        var battery = Battery(socPercent: 50, powerW: 11_000);

        battery.Advance(TimeSpan.FromHours(1));

        Assert.Equal(72, battery.SoCPercent, 6);
    }

    [Fact]
    public void A_battery_never_charges_past_full()
    {
        var battery = Battery(socPercent: 90, powerW: 11_000);

        battery.Advance(TimeSpan.FromHours(1));

        Assert.Equal(100, battery.SoCPercent, 6);
    }

    [Fact]
    public void Only_the_energy_that_fits_is_delivered()
    {
        var battery = Battery(socPercent: 90, powerW: 11_000);

        battery.Advance(TimeSpan.FromHours(1));

        Assert.Equal(5_000, battery.EnergyWh, 6);
    }

    [Fact]
    public void A_full_battery_takes_no_more()
    {
        var battery = Battery(socPercent: 90, powerW: 11_000);
        battery.Advance(TimeSpan.FromHours(1));

        battery.Advance(TimeSpan.FromHours(1));

        Assert.Equal(5_000, battery.EnergyWh, 6);
        Assert.Equal(100, battery.SoCPercent, 6);
    }

    [Fact]
    public void Zero_power_delivers_nothing()
    {
        var battery = Battery(powerW: 0);

        battery.Advance(TimeSpan.FromHours(1));

        Assert.Equal(0, battery.EnergyWh);
    }

    [Fact]
    public void Time_cannot_run_backwards()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Battery().Advance(TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public void Power_cannot_be_negative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BatteryModel(CapacityWh, 0, -1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Capacity_must_be_positive(double capacityWh)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BatteryModel(capacityWh, 0, 11_000));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void The_initial_state_of_charge_must_be_a_percentage(double socPercent)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BatteryModel(CapacityWh, socPercent, 11_000));
    }
}
