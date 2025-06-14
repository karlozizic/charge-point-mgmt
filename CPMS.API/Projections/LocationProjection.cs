using CPMS.API.Events.Location;
using Marten.Events.Aggregation;

namespace CPMS.API.Projections;

public class LocationProjection : SingleStreamProjection<LocationReadModel, Guid>
{
    public LocationProjection()
    {
        ProjectEvent<LocationCreatedEvent>((model, @event) => {
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
            return model;
        });

        ProjectEvent<LocationUpdatedEvent>((model, @event) => {
            model.Name = @event.Name;
            model.Address = @event.Address;
            model.City = @event.City;
            model.PostalCode = @event.PostalCode;
            model.Country = @event.Country;
            model.Latitude = @event.Latitude;
            model.Longitude = @event.Longitude;
            model.Description = @event.Description;
            return model;
        });

        ProjectEvent<ChargePointAddedToLocationEvent>((model, @event) => {
            model.ChargePointIds ??= new List<Guid>();
                
            if (!model.ChargePointIds.Contains(@event.ChargePointId))
            {
                model.ChargePointIds.Add(@event.ChargePointId);
            }
            return model;
        });
    }
}