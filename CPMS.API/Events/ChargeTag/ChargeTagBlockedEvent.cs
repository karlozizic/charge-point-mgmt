using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.ChargeTag;

public class ChargeTagBlockedEvent : DomainEventBase
{
    public Guid ChargeTagId { get; }
        
    public ChargeTagBlockedEvent(Guid chargeTagId)
    {
        ChargeTagId = chargeTagId;
    }
}