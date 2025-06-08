using CPMS.API.Events.Billing;
using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Entities;

public class SessionBilling : Entity, IAggregateRoot
{
    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public Guid PricingGroupId { get; private set; }
    public decimal BaseAmount { get; private set; }
    public decimal EnergyAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; }
    public string? StripePaymentIntentId { get; private set; }
    public string? StripeSessionId { get; private set; }
    public string PaymentStatus { get; private set; } = "pending";
    public DateTime? PaidAt { get; private set; }
    
    private SessionBilling() { }
    
    public SessionBilling(
        Guid sessionId,
        Guid pricingGroupId,
        decimal baseAmount,
        decimal energyAmount,
        string currency)
    {
        var totalAmount = baseAmount + energyAmount;
        
        var @event = new SessionBillingCalculatedEvent(
            sessionId, pricingGroupId, baseAmount, energyAmount, 
            totalAmount, currency, DateTime.UtcNow);
        
        AddDomainEvent(@event);
        Apply(@event);
    }
    
    private void Apply(SessionBillingCalculatedEvent @event)
    {
        Id = Guid.NewGuid();
        SessionId = @event.SessionId;
        PricingGroupId = @event.PricingGroupId;
        BaseAmount = @event.BaseAmount;
        EnergyAmount = @event.EnergyAmount;
        TotalAmount = @event.TotalAmount;
        Currency = @event.Currency;
    }
    
    public void SetPaymentIntent(string stripePaymentIntentId)
    {
        var @event = new PaymentIntentCreatedEvent(
            SessionId, stripePaymentIntentId, TotalAmount, 
            Currency, "requires_payment_method", DateTime.UtcNow);
        
        AddDomainEvent(@event);
        Apply(@event);
    }
    
    private void Apply(PaymentIntentCreatedEvent @event)
    {
        StripePaymentIntentId = @event.StripePaymentIntentId;
        PaymentStatus = @event.Status;
    }
    
    public void MarkAsPaid()
    {
        if (PaymentStatus == "succeeded")
            return;
            
        var @event = new PaymentCompletedEvent(
            SessionId, StripePaymentIntentId!, TotalAmount, DateTime.UtcNow);
        
        AddDomainEvent(@event);
        Apply(@event);
    }
    
    private void Apply(PaymentCompletedEvent @event)
    {
        PaymentStatus = "succeeded";
        PaidAt = @event.PaidAt;
    }
    
    public void SetStripeSessionId(string stripeSessionId)
    {
        var @event = new StripeSessionCreatedEvent(
            SessionId, stripeSessionId, TotalAmount, Currency, DateTime.UtcNow);
        
        AddDomainEvent(@event);
        Apply(@event);
    }
    
    private void Apply(StripeSessionCreatedEvent @event)
    {
        StripeSessionId = @event.StripeSessionId;
        PaymentStatus = "pending_payment";
    }
}