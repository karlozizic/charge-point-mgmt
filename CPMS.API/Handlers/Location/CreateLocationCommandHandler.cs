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
    private readonly IAggregateRepository<Entities.Location> _locations;

    public CreateLocationCommandHandler(IAggregateRepository<Entities.Location> locations)
    {
        _locations = locations;
    }

    public async Task<Guid> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
    {
        var location = new Entities.Location(
            Guid.NewGuid(),
            command.Name,
            command.Address,
            command.City,
            command.PostalCode,
            command.Country,
            command.Latitude,
            command.Longitude,
            command.Description);

        await _locations.SaveAsync(location, cancellationToken);
        return location.Id;
    }
}
