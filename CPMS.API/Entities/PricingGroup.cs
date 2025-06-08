using CPMS.API.Events.PricingGroup;
using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Entities;

public class PricingGroup : Entity, IAggregateRoot
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal BasePrice { get; private set; }
    public decimal PricePerKwh { get; private set; }
    public string Currency { get; private set; }
    public bool IsActive { get; private set; }
    
    private readonly List<Guid> _chargePointIds = new();
    public IReadOnlyCollection<Guid> ChargePointIds => _chargePointIds.AsReadOnly();
    
    private PricingGroup() { }
    
    public PricingGroup(
        Guid id,
        string name,
        decimal basePrice,
        decimal pricePerKwh,
        string currency = "EUR")
    {
        var @event = new PricingGroupCreatedEvent(
            id, name, basePrice, pricePerKwh, currency, true);
        
        AddDomainEvent(@event);
        Apply(@event);
    }
    
    private void Apply(PricingGroupCreatedEvent @event)
    {
        Id = @event.PricingGroupId;
        Name = @event.Name;
        BasePrice = @event.BasePrice;
        PricePerKwh = @event.PricePerKwh;
        Currency = @event.Currency;
        IsActive = @event.IsActive;
    }
    
    public void AssignChargePoint(Guid chargePointId)
    {
        if (_chargePointIds.Contains(chargePointId))
            return;
            
        var @event = new ChargePointAssignedToPricingGroupEvent(
            Id, chargePointId, DateTime.UtcNow);
        
        AddDomainEvent(@event);
        Apply(@event);
    }
    
    private void Apply(ChargePointAssignedToPricingGroupEvent @event)
    {
        _chargePointIds.Add(@event.ChargePointId);
    }
    
    public decimal CalculateSessionCost(double energyConsumed)
    {
        return BasePrice + PricePerKwh * (decimal)energyConsumed;
    }
}