using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.ChargeTag;

public class ChargeTagCreatedEvent : DomainEventBase
{
    public Guid ChargeTagId { get; }
    public string TagId { get; }
    public string TagName { get; }
    public string ParentTagId { get; }
    public DateTime? ExpiryDate { get; }
    public bool Blocked { get; }
        
    public ChargeTagCreatedEvent(
        Guid chargeTagId,
        string tagId,
        string tagName,
        string parentTagId,
        DateTime? expiryDate,
        bool blocked)
    {
        ChargeTagId = chargeTagId;
        TagId = tagId;
        TagName = tagName;
        ParentTagId = parentTagId;
        ExpiryDate = expiryDate;
        Blocked = blocked;
    }
}