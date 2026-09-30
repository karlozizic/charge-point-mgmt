using CPMS.API.Events.PricingGroup;
using Marten.Events.Aggregation;

namespace CPMS.API.Projections;

public class PricingGroupProjection : SingleStreamProjection<PricingGroupReadModel, Guid>
{
    public void Apply(PricingGroupCreatedEvent @event, PricingGroupReadModel model)
    {
        model.Id = @event.PricingGroupId;
        model.Name = @event.Name;
        model.BasePrice = @event.BasePrice;
        model.PricePerKwh = @event.PricePerKwh;
        model.Currency = @event.Currency;
        model.IsActive = @event.IsActive;
        model.ChargePointIds = new List<Guid>();
    }

    public void Apply(ChargePointAssignedToPricingGroupEvent @event, PricingGroupReadModel model)
    {
        if (!model.ChargePointIds.Contains(@event.ChargePointId))
        {
            model.ChargePointIds.Add(@event.ChargePointId);
        }
    }
}
