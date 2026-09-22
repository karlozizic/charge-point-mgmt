namespace CPMS.Simulator;

// Energy delivered and the resulting state of charge. Takes elapsed time rather than reading a
// clock, so a test can advance an hour without waiting one.
public sealed class BatteryModel
{
    private readonly double _capacityWh;
    private readonly double _initialSoCPercent;
    private readonly double _powerW;
    private readonly double _headroomWh;

    public BatteryModel(double capacityWh, double initialSoCPercent, double powerW)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacityWh);
        ArgumentOutOfRangeException.ThrowIfNegative(initialSoCPercent);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(initialSoCPercent, 100);
        ArgumentOutOfRangeException.ThrowIfNegative(powerW);

        _capacityWh = capacityWh;
        _initialSoCPercent = initialSoCPercent;
        _powerW = powerW;
        _headroomWh = capacityWh * (100 - initialSoCPercent) / 100;
    }

    public double EnergyWh { get; private set; }

    public double SoCPercent => Math.Min(100, _initialSoCPercent + EnergyWh / _capacityWh * 100);

    public void Advance(TimeSpan elapsed)
    {
        if (elapsed < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(elapsed));

        EnergyWh = Math.Min(_headroomWh, EnergyWh + _powerW * elapsed.TotalHours);
    }
}
