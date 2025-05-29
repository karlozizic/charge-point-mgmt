using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.ChargeTag;

public class ChargeTagIdUpdatedEvent : DomainEventBase
{
    public Guid ChargeTagId { get; }
    public string TagId { get; }
        
    public ChargeTagIdUpdatedEvent(
        Guid chargeTagId,
        string tagId)
    {
        ChargeTagId = chargeTagId;
        TagId = tagId;
    }
}