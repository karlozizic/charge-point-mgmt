using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.ChargeTag;

public class ChargeTagExpiryUpdatedEvent : DomainEventBase
{
    public Guid ChargeTagId { get; }
    public DateTime? ExpiryDate { get; }
        
    public ChargeTagExpiryUpdatedEvent(Guid chargeTagId, DateTime? expiryDate)
    {
        ChargeTagId = chargeTagId;
        ExpiryDate = expiryDate;
    }
}