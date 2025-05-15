using CPMS.API.Repositories;
using CPMS.BuildingBlocks.Infrastructure.Logger;
using MediatR;

namespace CPMS.API.Handlers.Connector;

public class AddConnectorCommand : IRequest
{
    public string OcppChargerId { get; set; }
    public string Name { get; set; }
}
    
public class AddConnectorCommandHandler : IRequestHandler<AddConnectorCommand>
{
    private readonly IChargePointRepository _repository;
    private readonly ILoggerService _logger;
        
    public AddConnectorCommandHandler(IChargePointRepository repository,
        ILoggerService logger)
    {
        _repository = repository;
        _logger = logger;
    }
        
    public async Task Handle(AddConnectorCommand command, CancellationToken cancellationToken)
    {
        var chargePointId = command.OcppChargerId;
        var chargePoint = await _repository.GetByOcppChargerIdAsync(chargePointId);

        if (chargePoint == null)
        {
            _logger.Info($"Charge point with id {chargePointId} not found");
            return;
        }

        int connectorId = chargePoint.Connectors.Count > 0 
            ? chargePoint.Connectors.Max(c => c.Id) + 1 
            : 1;
        
        chargePoint.AddConnector(connectorId, command.Name);
            
        await _repository.UpdateAsync(chargePoint);
    }
}