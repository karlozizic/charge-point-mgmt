using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.ChargeTag;

public class ChargeTagUnblockedEvent : DomainEventBase
{
    public Guid ChargeTagId { get; }
        
    public ChargeTagUnblockedEvent(Guid chargeTagId)
    {
        ChargeTagId = chargeTagId;
    }
}
