using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.ChargePoint;

public class ConnectorAddedEvent : DomainEventBase
{
    public Guid ChargePointId { get; }
    public int ConnectorId { get; }
    public string ConnectorName { get; }
        
    public ConnectorAddedEvent(Guid chargePointId, int connectorId, string connectorName)
    {
        ChargePointId = chargePointId;
        ConnectorId = connectorId;
        ConnectorName = connectorName;
    }
}