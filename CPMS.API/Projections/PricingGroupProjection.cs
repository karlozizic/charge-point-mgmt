using CPMS.API.Events.PricingGroup;
using Marten.Events.Aggregation;

namespace CPMS.API.Projections;

public class PricingGroupProjection : SingleStreamProjection<PricingGroupReadModel, Guid>
{
    public PricingGroupProjection()
    {
        ProjectEvent<PricingGroupCreatedEvent>((model, @event) => {
            model.Id = @event.PricingGroupId;
            model.Name = @event.Name;
            model.BasePrice = @event.BasePrice;
            model.PricePerKwh = @event.PricePerKwh;
            model.Currency = @event.Currency;
            model.IsActive = @event.IsActive;
            model.ChargePointIds = new List<Guid>();
            return model;
        });
        
        ProjectEvent<ChargePointAssignedToPricingGroupEvent>((model, @event) => {
            if (!model.ChargePointIds.Contains(@event.ChargePointId))
            {
                model.ChargePointIds.Add(@event.ChargePointId);
            }
            return model;
        });
    }
}