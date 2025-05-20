using CPMS.API.Repositories;
using CPMS.BuildingBlocks.Infrastructure.Logger;
using MediatR;

namespace CPMS.API.Handlers.Connector;

public class UpdateConnectorStatusCommand : IRequest
{
    public string OcppChargerId { get; set; }
    public Guid ChargePointId { get; set; }
    public int ConnectorId { get; set; }
    public string Status { get; set; }
    public string ErrorCode { get; set; }
    public string Info { get; set; }
    public DateTime? Timestamp { get; set; }
}
    
public class UpdateConnectorStatusCommandHandler : IRequestHandler<UpdateConnectorStatusCommand>
{
    private readonly IChargePointRepository _chargePointRepository;
    private readonly ILoggerService _logger;
        
    public UpdateConnectorStatusCommandHandler(IChargePointRepository chargePointRepository,
        ILoggerService logger)
    {
        _chargePointRepository = chargePointRepository;
        _logger = logger;
    }

    public async Task Handle(UpdateConnectorStatusCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var chargePoint = await _chargePointRepository.GetByOcppChargerIdAsync(command.OcppChargerId);

            if (chargePoint == null)
            {
                _logger.Warning($"Received status notification for unknown charge point: {command.ChargePointId}");
                return;
            }

            var connector = chargePoint.Connectors.FirstOrDefault(c => c.Id == command.ConnectorId);

            if (connector == null)
            {
                int connectorId = chargePoint.Connectors.Count > 0 
                    ? chargePoint.Connectors.Max(c => c.Id) + 1 
                    : 1;
                
                chargePoint.AddConnector(connectorId, $"Connector {command.ConnectorId}");

                connector = chargePoint.Connectors.FirstOrDefault(c => c.Id == command.ConnectorId);
            }

            chargePoint.UpdateConnectorStatus(connector.Id, command.Status);

            if (!string.IsNullOrEmpty(command.ErrorCode) && command.ErrorCode != "NoError")
                chargePoint.LogConnectorError(connector.Id, command.ErrorCode, command.Info);

            await _chargePointRepository.UpdateAsync(chargePoint);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error updating connector status for {command.ChargePointId}:{command.ConnectorId}");
            throw;
        }
    }
}