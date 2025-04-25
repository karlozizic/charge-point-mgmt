using CPMS.API.Repositories;
using MediatR;

namespace CPMS.API.Handlers.Connector;

public class AddConnectorCommand : IRequest
{
    public Guid ChargePointId { get; set; }
    public string Name { get; set; }
}
    
public class AddConnectorCommandHandler : IRequestHandler<AddConnectorCommand>
{
    private readonly IChargePointRepository _repository;
        
    public AddConnectorCommandHandler(IChargePointRepository repository)
    {
        _repository = repository;
    }
        
    public async Task Handle(AddConnectorCommand command, CancellationToken cancellationToken)
    {
        var chargePointId = command.ChargePointId;
        var chargePoint = await _repository.GetByIdAsync(chargePointId);

        //todo
        if (chargePoint == null)
            return;

        var connectorId = Guid.NewGuid();
        chargePoint.AddConnector(connectorId, command.Name);
            
        await _repository.UpdateAsync(chargePoint);
    }
}