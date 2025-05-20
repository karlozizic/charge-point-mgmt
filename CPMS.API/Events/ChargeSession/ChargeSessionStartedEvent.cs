using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.ChargeSession;

public class ChargeSessionStartedEvent : DomainEventBase
{
    public Guid ChargeSessionId { get; }
    public int TransactionId { get; }
    public Guid ChargePointId { get; }
    public int ConnectorId { get; }
    public string TagId { get; }
    public DateTime StartTime { get; }
    public double StartMeterValue { get; }
        
    public ChargeSessionStartedEvent(
        Guid chargeSessionId,
        int transactionId,
        Guid chargePointId,
        int connectorId,
        string tagId,
        DateTime startTime,
        double startMeterValue)
    {
        ChargeSessionId = chargeSessionId;
        TransactionId = transactionId;
        ChargePointId = chargePointId;
        ConnectorId = connectorId;
        TagId = tagId;
        StartTime = startTime;
        StartMeterValue = startMeterValue;
    }
}