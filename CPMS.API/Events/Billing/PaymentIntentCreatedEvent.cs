using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.Billing;

public class PaymentIntentCreatedEvent : DomainEventBase
{
    public Guid SessionId { get; }
    public string StripePaymentIntentId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string Status { get; }
    public DateTime CreatedAt { get; }
    
    public PaymentIntentCreatedEvent(
        Guid sessionId,
        string stripePaymentIntentId,
        decimal amount,
        string currency,
        string status,
        DateTime createdAt)
    {
        SessionId = sessionId;
        StripePaymentIntentId = stripePaymentIntentId;
        Amount = amount;
        Currency = currency;
        Status = status;
        CreatedAt = createdAt;
    }
}