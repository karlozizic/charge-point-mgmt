using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.ChargeSession;

public class ChargeSessionStoppedEvent : DomainEventBase
{
    public Guid ChargeSessionId { get; }
    public Guid ChargePointId { get; }
    public int ConnectorId { get; }
    public string TagId { get; }
    public DateTime StopTime { get; }
    public double StopMeterValue { get; }
    public string StopReason { get; }
        
    public ChargeSessionStoppedEvent(
        Guid chargeSessionId,
        Guid chargePointId,
        int connectorId,
        string tagId,
        DateTime stopTime,
        double stopMeterValue,
        string stopReason)
    {
        ChargeSessionId = chargeSessionId;
        ChargePointId = chargePointId;
        ConnectorId = connectorId;
        TagId = tagId;
        StopTime = stopTime;
        StopMeterValue = stopMeterValue;
        StopReason = stopReason;
    }
}