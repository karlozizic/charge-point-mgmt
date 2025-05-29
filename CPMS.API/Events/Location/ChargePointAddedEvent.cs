using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.Location;

public class ChargePointAddedToLocationEvent : DomainEventBase
{
    public Guid LocationId { get; }
    public Guid ChargePointId { get; }
    public string OcppChargerId { get; }
    public DateTime AddedAt { get; }

    public ChargePointAddedToLocationEvent(Guid locationId, Guid chargePointId, 
        string ocppChargerId, DateTime addedAt)
    {
        LocationId = locationId;
        ChargePointId = chargePointId;
        OcppChargerId = ocppChargerId;
        AddedAt = addedAt;
    }
}
