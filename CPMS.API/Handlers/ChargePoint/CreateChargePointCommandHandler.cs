using CPMS.API.Repositories;
using MediatR;

namespace CPMS.API.Handlers.ChargePoint;

public class CreateChargePointCommand : IRequest<Guid>
{
    public string Name { get; set; }
    public Guid LocationId { get; set; }
    public double? MaxPower { get; set; }
}

public class CreateChargePointCommandHandler : IRequestHandler<CreateChargePointCommand, Guid>
{
    private readonly IChargePointRepository _repository;
        
    public CreateChargePointCommandHandler(IChargePointRepository repository)
    {
        _repository = repository;
    }
        
    public async Task<Guid> Handle(CreateChargePointCommand command, CancellationToken cancellationToken)
    {
        var chargePointId = Guid.NewGuid();
        var locationId = command.LocationId;
            
        var chargePoint = new Entities.ChargePoint(
            chargePointId, 
            command.Name, 
            locationId, 
            command.MaxPower,
            null);
        
        await _repository.AddAsync(chargePoint);

        return chargePointId;
    }
}