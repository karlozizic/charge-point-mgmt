using CPMS.API.Events.ChargeTag;
using Marten.Events.Aggregation;

namespace CPMS.API.Projections;

public class ChargeTagProjection : SingleStreamProjection<ChargeTagReadModel>
{
    public ChargeTagProjection()
    {
        ProjectEvent<ChargeTagCreatedEvent>((model, @event) => {
            model.Id = @event.ChargeTagId;
            model.TagId = @event.TagId;
            model.TagName = @event.TagName;
            model.ParentTagId = @event.ParentTagId;
            model.ExpiryDate = @event.ExpiryDate;
            model.Blocked = @event.Blocked;
            return model;
        });
            
        ProjectEvent<ChargeTagBlockedEvent>((model, @event) => {
            model.Blocked = true;
            return model;
        });
            
        ProjectEvent<ChargeTagUnblockedEvent>((model, @event) => {
            model.Blocked = false;
            return model;
        });
            
        ProjectEvent<ChargeTagExpiryUpdatedEvent>((model, @event) => {
            model.ExpiryDate = @event.ExpiryDate;
            return model;
        });
    }
}