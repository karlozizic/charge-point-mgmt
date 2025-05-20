using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.ChargeSession;

public class MeterValueRecordedEvent : DomainEventBase
{
    public Guid ChargeSessionId { get; }
    public int TransactionId { get; }
    public Guid ChargePointId { get; }
    public int ConnectorId { get; }
    public double? CurrentPower { get; }
    public double? EnergyConsumed { get; }
    public double? StateOfCharge { get; }
    public DateTime Timestamp { get; }
        
    public MeterValueRecordedEvent(
        Guid chargeSessionId,
        int transactionId,
        Guid chargePointId,
        int connectorId,
        DateTime timestamp,
        double? currentPower = null,
        double? energyConsumed = null,
        double? stateOfCharge = null)
    {
        ChargeSessionId = chargeSessionId;
        TransactionId = transactionId;
        ChargePointId = chargePointId;
        ConnectorId = connectorId;
        Timestamp = timestamp;
        CurrentPower = currentPower;
        EnergyConsumed = energyConsumed;
        StateOfCharge = stateOfCharge;
    }
}