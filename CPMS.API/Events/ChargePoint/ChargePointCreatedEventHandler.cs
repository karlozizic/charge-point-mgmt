using CPMS.API.Repositories;
using MediatR;

namespace CPMS.API.Events.ChargePoint;

public class ChargePointCreatedEventHandler : INotificationHandler<ChargePointCreatedEvent>
{
    private readonly IAggregateRepository<Entities.Location> _locations;

    public ChargePointCreatedEventHandler(IAggregateRepository<Entities.Location> locations)
    {
        _locations = locations;
    }

    public async Task Handle(ChargePointCreatedEvent notification, CancellationToken cancellationToken)
    {
        var location = await _locations.LoadAsync(notification.LocationId, cancellationToken);
        if (location == null)
            return;

        location.AddChargePoint(notification.ChargePointId, notification.OcppChargerId);
        await _locations.SaveAsync(location, cancellationToken);
    }
}
