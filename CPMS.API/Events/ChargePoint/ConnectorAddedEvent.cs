using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.ChargePoint;

public class ConnectorAddedEvent : DomainEventBase
{
    public Guid ChargePointId { get; }
    public int ConnectorId { get; }
    public string ConnectorName { get; }
    public DateTime AddedAt { get; }

    public ConnectorAddedEvent(Guid chargePointId, int connectorId, string connectorName, DateTime addedAt)
    {
        ChargePointId = chargePointId;
        ConnectorId = connectorId;
        ConnectorName = connectorName;
        AddedAt = addedAt;
    }
}