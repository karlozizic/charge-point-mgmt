using CPMS.API.Events.Billing;
using Marten.Events.Aggregation;

namespace CPMS.API.Projections;

public class SessionBillingProjection : SingleStreamProjection<SessionBillingReadModel, Guid>
{
    public SessionBillingProjection()
    {
        ProjectEvent<SessionBillingCalculatedEvent>((model, @event) => {
            model.Id = Guid.NewGuid();
            model.SessionId = @event.SessionId;
            model.PricingGroupId = @event.PricingGroupId;
            model.BaseAmount = @event.BaseAmount;
            model.EnergyAmount = @event.EnergyAmount;
            model.TotalAmount = @event.TotalAmount;
            model.Currency = @event.Currency;
            model.PaymentStatus = "pending";
            model.CreatedAt = @event.CalculatedAt;
            return model;
        });
        
        ProjectEvent<PaymentIntentCreatedEvent>((model, @event) => {
            model.StripePaymentIntentId = @event.StripePaymentIntentId;
            model.PaymentStatus = @event.Status;
            return model;
        });
        
        ProjectEvent<StripeSessionCreatedEvent>((model, @event) => {
            model.StripeSessionId = @event.StripeSessionId;
            model.PaymentStatus = "pending_payment";
            return model;
        });
        
        ProjectEvent<PaymentCompletedEvent>((model, @event) => {
            model.PaymentStatus = "succeeded";
            model.PaidAt = @event.PaidAt;
            return model;
        });
    }
}
