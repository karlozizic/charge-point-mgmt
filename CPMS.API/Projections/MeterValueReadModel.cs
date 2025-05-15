namespace CPMS.API.Projections;

public class MeterValueReadModel
{
    public Guid SessionId { get; set; }
    public int TransactionId { get; set; }
    public double? CurrentPower { get; set; }
    public double? EnergyConsumed { get; set; }
    public double? StateOfCharge { get; set; }
    public DateTime Timestamp { get; set; }
}