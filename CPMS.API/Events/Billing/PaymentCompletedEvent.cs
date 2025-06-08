using CPMS.BuildingBlocks.Domain;

namespace CPMS.API.Events.Billing;

public class PaymentCompletedEvent : DomainEventBase
{
    public Guid SessionId { get; }
    public string StripePaymentIntentId { get; }
    public decimal AmountPaid { get; }
    public DateTime PaidAt { get; }
    
    public PaymentCompletedEvent(
        Guid sessionId,
        string stripePaymentIntentId,
        decimal amountPaid,
        DateTime paidAt)
    {
        SessionId = sessionId;
        StripePaymentIntentId = stripePaymentIntentId;
        AmountPaid = amountPaid;
        PaidAt = paidAt;
    }
}