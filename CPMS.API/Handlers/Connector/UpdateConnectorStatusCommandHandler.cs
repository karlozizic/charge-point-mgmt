using CPMS.API.Projections;
using CPMS.API.Repositories;
using CPMS.BuildingBlocks.Infrastructure.Logger;
using Marten;
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
    private readonly IAggregateRepository<Entities.ChargePoint> _chargePoints;
    private readonly IQuerySession _querySession;
    private readonly ILoggerService _logger;

    public UpdateConnectorStatusCommandHandler(
        IAggregateRepository<Entities.ChargePoint> chargePoints,
        IQuerySession querySession,
        ILoggerService logger)
    {
        _chargePoints = chargePoints;
        _querySession = querySession;
        _logger = logger;
    }

    public async Task Handle(UpdateConnectorStatusCommand command, CancellationToken cancellationToken)
    {
        var readModel = await _querySession.Query<ChargePointReadModel>()
            .FirstOrDefaultAsync(cp => cp.OcppChargerId == command.OcppChargerId, cancellationToken);
        var chargePoint = readModel == null ? null : await _chargePoints.LoadAsync(readModel.Id, cancellationToken);

        if (chargePoint == null)
        {
            _logger.Warning($"Received status notification for unknown charge point: {command.OcppChargerId}");
            return;
        }

        // A charger may report a connector the operator never registered; create it on first sight.
        if (chargePoint.Connectors.All(c => c.Id != command.ConnectorId))
            chargePoint.AddConnector(command.ConnectorId, $"Connector {command.ConnectorId}");

        chargePoint.UpdateConnectorStatus(command.ConnectorId, command.Status);

        if (!string.IsNullOrEmpty(command.ErrorCode) && command.ErrorCode != "NoError")
            chargePoint.LogConnectorError(command.ConnectorId, command.ErrorCode, command.Info);

        await _chargePoints.SaveAsync(chargePoint, cancellationToken);
    }
}
