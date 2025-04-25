using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.ChargePoint;

public class ConnectorAddedEvent : DomainEventBase
{
    public Guid ChargePointId { get; }
    public Guid ConnectorId { get; }
    public string ConnectorName { get; }
        
    public ConnectorAddedEvent(Guid chargePointId, Guid connectorId, string connectorName)
    {
        ChargePointId = chargePointId;
        ConnectorId = connectorId;
        ConnectorName = connectorName;
    }
}