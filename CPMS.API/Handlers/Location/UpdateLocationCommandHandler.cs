using CPMS.API.Exceptions;
using CPMS.API.Projections;
using CPMS.API.Repositories;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.Location;

public class UpdateLocationCommand : IRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Description { get; set; }
}

public class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand>
{
    private readonly IAggregateRepository<Entities.Location> _locations;
    private readonly IQuerySession _querySession;

    public UpdateLocationCommandHandler(
        IAggregateRepository<Entities.Location> locations,
        IQuerySession querySession)
    {
        _locations = locations;
        _querySession = querySession;
    }

    public async Task Handle(UpdateLocationCommand command, CancellationToken cancellationToken)
    {
        var location = await _locations.LoadAsync(command.Id, cancellationToken);
        if (location == null)
            throw new NotFoundException($"Location with ID {command.Id} does not exist.");

        var nameTaken = await _querySession.Query<LocationReadModel>()
            .AnyAsync(l => l.Name == command.Name && l.Id != command.Id, cancellationToken);
        if (nameTaken)
            throw new InvalidOperationException($"Another location with name '{command.Name}' already exists");

        location.UpdateDetails(
            command.Name,
            command.Address,
            command.City,
            command.PostalCode,
            command.Country,
            command.Latitude,
            command.Longitude,
            command.Description);

        await _locations.SaveAsync(location, cancellationToken);
    }
}
