using CPMS.API.Repositories;
using MediatR;

namespace CPMS.API.Events.ChargePoint;

public class ChargePointCreatedEventHandler : INotificationHandler<ChargePointCreatedEvent>
{
    private readonly ILocationRepository _locationRepository;

    public ChargePointCreatedEventHandler(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task Handle(ChargePointCreatedEvent notification, CancellationToken cancellationToken)
    {
        var location = await _locationRepository.GetByIdAsync(notification.LocationId);
        
        if (location != null)
        {
            location.AddChargePoint(notification.ChargePointId, notification.OcppChargerId);
            await _locationRepository.UpdateAsync(location);
        }
    }
}