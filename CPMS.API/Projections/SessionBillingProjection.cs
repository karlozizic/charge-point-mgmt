using CPMS.API.Events.Billing;
using Marten.Events.Aggregation;

namespace CPMS.API.Projections;

public class SessionBillingProjection : SingleStreamProjection<SessionBillingReadModel, Guid>
{
    public void Apply(SessionBillingCalculatedEvent @event, SessionBillingReadModel model)
    {
        model.Id = @event.BillingId;
        model.SessionId = @event.SessionId;
        model.PricingGroupId = @event.PricingGroupId;
        model.BaseAmount = @event.BaseAmount;
        model.EnergyAmount = @event.EnergyAmount;
        model.TotalAmount = @event.TotalAmount;
        model.Currency = @event.Currency;
        model.PaymentStatus = "pending";
        model.CreatedAt = @event.CalculatedAt;
    }

    public void Apply(PaymentIntentCreatedEvent @event, SessionBillingReadModel model)
    {
        model.StripePaymentIntentId = @event.StripePaymentIntentId;
        model.PaymentStatus = @event.Status;
    }

    public void Apply(StripeSessionCreatedEvent @event, SessionBillingReadModel model)
    {
        model.StripeSessionId = @event.StripeSessionId;
        model.PaymentStatus = "pending_payment";
    }

    public void Apply(PaymentCompletedEvent @event, SessionBillingReadModel model)
    {
        model.PaymentStatus = "succeeded";
        model.PaidAt = @event.PaidAt;
    }
}
