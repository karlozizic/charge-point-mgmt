using CPMS.API.Events.ChargeTag;
using Marten.Events.Aggregation;

namespace CPMS.API.Projections;

public class ChargeTagProjection : SingleStreamProjection<ChargeTagReadModel, Guid>
{
    public void Apply(ChargeTagCreatedEvent @event, ChargeTagReadModel model)
    {
        model.Id = @event.ChargeTagId;
        model.TagId = @event.TagId;
        model.ExpiryDate = @event.ExpiryDate;
        model.Blocked = @event.Blocked;
    }

    public void Apply(ChargeTagBlockedEvent @event, ChargeTagReadModel model)
    {
        model.Blocked = true;
    }

    public void Apply(ChargeTagUnblockedEvent @event, ChargeTagReadModel model)
    {
        model.Blocked = false;
    }

    public void Apply(ChargeTagExpiryUpdatedEvent @event, ChargeTagReadModel model)
    {
        model.ExpiryDate = @event.ExpiryDate;
    }

    public void Apply(ChargeTagIdUpdatedEvent @event, ChargeTagReadModel model)
    {
        model.TagId = @event.TagId;
    }
}
