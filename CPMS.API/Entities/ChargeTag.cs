using CPMS.API.Events.ChargeTag;
using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Entities;

public class ChargeTag : Entity, IAggregateRoot
{
    public Guid Id { get; private set; }
    public string TagId { get; private set; } // RFID tag ID string
    public DateTime? ExpiryDate { get; private set; }
    public bool Blocked { get; private set; }
    
    private ChargeTag() { }
    
    public ChargeTag(
        Guid id,
        string tagId,
        DateTime? expiryDate = null,
        bool blocked = false)
    {
        var @event = new ChargeTagCreatedEvent(
            id,
            tagId,
            expiryDate,
            blocked
        );
        
        AddDomainEvent(@event);
        Apply(@event);
    }
    
    private void Apply(ChargeTagCreatedEvent @event)
    {
        Id = @event.ChargeTagId;
        TagId = @event.TagId;
        ExpiryDate = @event.ExpiryDate;
        Blocked = @event.Blocked;
    }
    
    public void Block()
    {
        if (Blocked)
            return;
            
        var @event = new ChargeTagBlockedEvent(Id);
        
        AddDomainEvent(@event);
        Apply(@event);
    }
    
    private void Apply(ChargeTagBlockedEvent @event)
    {
        Blocked = true;
    }
    
    public void Unblock()
    {
        if (!Blocked)
            return;
            
        var @event = new ChargeTagUnblockedEvent(Id);
        
        AddDomainEvent(@event);
        Apply(@event);
    }
    
    private void Apply(ChargeTagUnblockedEvent @event)
    {
        Blocked = false;
    }
    
    public void UpdateExpiry(DateTime? expiryDate)
    {
        var @event = new ChargeTagExpiryUpdatedEvent(Id, expiryDate);
        
        AddDomainEvent(@event);
        Apply(@event);
    }
    
    private void Apply(ChargeTagExpiryUpdatedEvent @event)
    {
        ExpiryDate = @event.ExpiryDate;
    }
}