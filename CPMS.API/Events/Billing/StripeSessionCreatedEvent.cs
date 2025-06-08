using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.Billing;

public class StripeSessionCreatedEvent : DomainEventBase
{
    public Guid SessionId { get; }
    public string StripeSessionId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public DateTime CreatedAt { get; }
    
    public StripeSessionCreatedEvent(
        Guid sessionId,
        string stripeSessionId,
        decimal amount,
        string currency,
        DateTime createdAt)
    {
        SessionId = sessionId;
        StripeSessionId = stripeSessionId;
        Amount = amount;
        Currency = currency;
        CreatedAt = createdAt;
    }
}