using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.PricingGroup;

public class PricingGroupCreatedEvent : DomainEventBase
{
    public Guid PricingGroupId { get; }
    public string Name { get; }
    public decimal BasePrice { get; }
    public decimal PricePerKwh { get; }
    public string Currency { get; }
    public bool IsActive { get; }
    
    public PricingGroupCreatedEvent(
        Guid pricingGroupId,
        string name,
        decimal basePrice,
        decimal pricePerKwh,
        string currency,
        bool isActive)
    {
        PricingGroupId = pricingGroupId;
        Name = name;
        BasePrice = basePrice;
        PricePerKwh = pricePerKwh;
        Currency = currency;
        IsActive = isActive;
    }
}