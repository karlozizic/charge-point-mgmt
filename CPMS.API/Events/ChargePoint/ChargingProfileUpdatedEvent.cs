using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.ChargePoint;

public class ChargingProfileUpdatedEvent : DomainEventBase
{
    public Guid ChargePointId { get; }
    public int ProfileId { get; }
    public string ProfileData { get; }
        
    public ChargingProfileUpdatedEvent(
        Guid chargePointId,
        int profileId,
        string profileData)
    {
        ChargePointId = chargePointId;
        ProfileId = profileId;
        ProfileData = profileData;
    }
}