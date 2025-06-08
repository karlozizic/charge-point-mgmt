using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.Billing;

public class SessionBillingCalculatedEvent : DomainEventBase
{
    public Guid SessionId { get; }
    public Guid PricingGroupId { get; }
    public decimal BaseAmount { get; }
    public decimal EnergyAmount { get; }
    public decimal TotalAmount { get; }
    public string Currency { get; }
    public DateTime CalculatedAt { get; }
    
    public SessionBillingCalculatedEvent(
        Guid sessionId,
        Guid pricingGroupId,
        decimal baseAmount,
        decimal energyAmount,
        decimal totalAmount,
        string currency,
        DateTime calculatedAt)
    {
        SessionId = sessionId;
        PricingGroupId = pricingGroupId;
        BaseAmount = baseAmount;
        EnergyAmount = energyAmount;
        TotalAmount = totalAmount;
        Currency = currency;
        CalculatedAt = calculatedAt;
    }
}