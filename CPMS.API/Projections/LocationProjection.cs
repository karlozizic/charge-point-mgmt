using CPMS.API.Events.Location;
using Marten.Events.Aggregation;

namespace CPMS.API.Projections;

public class LocationProjection : SingleStreamProjection<LocationReadModel, Guid>
{
    public void Apply(LocationCreatedEvent @event, LocationReadModel model)
    {
        model.Id = @event.LocationId;
        model.Name = @event.Name;
        model.Address = @event.Address;
        model.City = @event.City;
        model.PostalCode = @event.PostalCode;
        model.Country = @event.Country;
        model.Latitude = @event.Latitude;
        model.Longitude = @event.Longitude;
        model.Description = @event.Description;
        model.CreatedAt = @event.CreatedAt;
        model.ChargePointIds = new List<Guid>();
    }

    public void Apply(LocationUpdatedEvent @event, LocationReadModel model)
    {
        model.Name = @event.Name;
        model.Address = @event.Address;
        model.City = @event.City;
        model.PostalCode = @event.PostalCode;
        model.Country = @event.Country;
        model.Latitude = @event.Latitude;
        model.Longitude = @event.Longitude;
        model.Description = @event.Description;
    }

    public void Apply(ChargePointAddedToLocationEvent @event, LocationReadModel model)
    {
        model.ChargePointIds ??= new List<Guid>();

        if (!model.ChargePointIds.Contains(@event.ChargePointId))
        {
            model.ChargePointIds.Add(@event.ChargePointId);
        }
    }
}
