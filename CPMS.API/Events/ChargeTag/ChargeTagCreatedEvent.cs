using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.ChargeTag;

public class ChargeTagCreatedEvent : DomainEventBase
{
    public Guid ChargeTagId { get; }
    public string TagId { get; }
    public DateTime? ExpiryDate { get; }
    public bool Blocked { get; }
        
    public ChargeTagCreatedEvent(
        Guid chargeTagId,
        string tagId,
        DateTime? expiryDate,
        bool blocked)
    {
        ChargeTagId = chargeTagId;
        TagId = tagId;
        ExpiryDate = expiryDate;
        Blocked = blocked;
    }
}