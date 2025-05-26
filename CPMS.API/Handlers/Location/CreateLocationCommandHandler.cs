using CPMS.API.Repositories;
using MediatR;

namespace CPMS.API.Handlers.Location;

public class CreateLocationCommand : IRequest<Guid>
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Description { get; set; }
}

public class CreateLocationCommandHandler : IRequestHandler<CreateLocationCommand, Guid>
{
    private readonly ILocationRepository _locationRepository;

    public CreateLocationCommandHandler(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }
    
    public async Task<Guid> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        var locationId = Guid.NewGuid();

        var location = new Entities.Location(
            locationId,
            command.Name,
            command.Address,
            command.City,
            command.PostalCode,
            command.Country,
            command.Latitude,
            command.Longitude,
            command.Description);

        await _locationRepository.AddAsync(location);
        return locationId;
    }
}