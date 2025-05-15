using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Entities;

public class MeterValue : ValueObject
{
    public Guid SessionId { get; set; }
    public double? CurrentPower { get; set; }
    public double? EnergyConsumed { get; set; }
    public double? StateOfCharge { get; set; }
    public DateTime Timestamp { get; }
    
    public MeterValue(DateTime timestamp,
        Guid sessionId,
        double? currentPower = null,
        double? energyConsumed = null,
        double? stateOfCharge = null)
    {
        SessionId = sessionId;
        CurrentPower = currentPower;
        EnergyConsumed = energyConsumed;
        StateOfCharge = stateOfCharge;
        Timestamp = timestamp;
    }
}