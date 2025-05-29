using CPMS.API.Exceptions;
using CPMS.API.Repositories;
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
    private readonly ILocationRepository _repository;

    public UpdateLocationCommandHandler(ILocationRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(UpdateLocationCommand command, CancellationToken cancellationToken)
    {
        var location = await _repository.GetByIdAsync(command.Id);
        
        if (location == null)
            throw new NotFoundException($"Location with ID {command.Id} does not exist.");
        
        var existingLocation = await _repository.GetByNameAsync(command.Name);
        if (existingLocation != null && existingLocation.Id != command.Id)
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

        await _repository.UpdateAsync(location);
    }
}
