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
            model.ExpiryDate = @event.ExpiryDate;
            model.Blocked = @event.Blocked;
        });
            
        ProjectEvent<ChargeTagBlockedEvent>((model, @event) => {
            model.Blocked = true;
        });
            
        ProjectEvent<ChargeTagUnblockedEvent>((model, @event) => {
            model.Blocked = false;
        });
            
        ProjectEvent<ChargeTagExpiryUpdatedEvent>((model, @event) => {
            model.ExpiryDate = @event.ExpiryDate;
        });
    }
}