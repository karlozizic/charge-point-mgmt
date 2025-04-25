using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.ChargePoint;

public class ConnectorStatusChangedEvent : DomainEventBase
{
    public Guid ChargePointId { get; }
    public Guid ConnectorId { get; }
    public string Status { get; }
    public DateTime Timestamp { get; }
        
    public ConnectorStatusChangedEvent(
        Guid chargePointId, 
        Guid connectorId, 
        string status, 
        DateTime timestamp)
    {
        ChargePointId = chargePointId;
        ConnectorId = connectorId;
        Status = status;
        Timestamp = timestamp;
    }
}