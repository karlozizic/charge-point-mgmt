using CPMS.API.Exceptions;
using CPMS.API.Projections;
using CPMS.API.Repositories;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.Connector;

public class AddConnectorCommand : IRequest
{
    public string OcppChargerId { get; set; }
    public string Name { get; set; }
}

public class AddConnectorCommandHandler : IRequestHandler<AddConnectorCommand>
{
    private readonly IAggregateRepository<Entities.ChargePoint> _chargePoints;
    private readonly IQuerySession _querySession;

    public AddConnectorCommandHandler(
        IAggregateRepository<Entities.ChargePoint> chargePoints,
        IQuerySession querySession)
    {
        _chargePoints = chargePoints;
        _querySession = querySession;
    }

    public async Task Handle(AddConnectorCommand command, CancellationToken cancellationToken)
    {
        var readModel = await _querySession.Query<ChargePointReadModel>()
            .FirstOrDefaultAsync(cp => cp.OcppChargerId == command.OcppChargerId, cancellationToken);
        var chargePoint = readModel == null ? null : await _chargePoints.LoadAsync(readModel.Id, cancellationToken);

        if (chargePoint == null)
            throw new NotFoundException($"Charge point {command.OcppChargerId} not found");

        chargePoint.AddConnector(chargePoint.NextConnectorId(), command.Name);

        await _chargePoints.SaveAsync(chargePoint, cancellationToken);
    }
}
