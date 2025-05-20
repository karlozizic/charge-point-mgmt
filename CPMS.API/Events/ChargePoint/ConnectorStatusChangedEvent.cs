using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.ChargePoint;

public class ConnectorStatusChangedEvent : DomainEventBase
{
    public Guid ChargePointId { get; }
    public int ConnectorId { get; }
    public string Status { get; }
    public DateTime Timestamp { get; }
        
    public ConnectorStatusChangedEvent(
        Guid chargePointId, 
        int connectorId, 
        string status, 
        DateTime timestamp)
    {
        ChargePointId = chargePointId;
        ConnectorId = connectorId;
        Status = status;
        Timestamp = timestamp;
    }
}