using CPMS.API.Repositories;
using MediatR;

namespace CPMS.API.Handlers.Connector;

public class UpdateConnectorStatusCommand : IRequest
{
    public Guid ChargePointId { get; set; }
    public int ConnectorId { get; set; }
    public string Status { get; set; }
}
    
public class UpdateConnectorStatusCommandHandler : IRequestHandler<UpdateConnectorStatusCommand>
{
    private readonly IChargePointRepository _repository;
        
    public UpdateConnectorStatusCommandHandler(IChargePointRepository repository)
    {
        _repository = repository;
    }
        
    public async Task Handle(UpdateConnectorStatusCommand command, CancellationToken cancellationToken)
    {
        var chargePointId = command.ChargePointId;
        var chargePoint = await _repository.GetByIdAsync(chargePointId);

        //todo
        if (chargePoint == null)
            return;                

        var connector = chargePoint.Connectors.FirstOrDefault(c => 
            int.Parse(c.Id.ToString().Substring(0, 8), System.Globalization.NumberStyles.HexNumber) == command.ConnectorId);
                
        //todo
        if (connector == null)
            return;  
            
        chargePoint.UpdateConnectorStatus(connector.Id, command.Status);
            
        await _repository.UpdateAsync(chargePoint);
    }
}