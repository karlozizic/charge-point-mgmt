using CPMS.API.BusinessRules;
using CPMS.API.Projections;
using CPMS.API.Repositories;
using CPMS.BuildingBlocks.Domain;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargePoint;

public class CreateChargePointCommand : IRequest<Guid>
{
    public string OcppChargerId { get; set; }
    public Guid LocationId { get; set; }
    public double? MaxPower { get; set; }
}

public class CreateChargePointCommandHandler : IRequestHandler<CreateChargePointCommand, Guid>
{
    private readonly IAggregateRepository<Entities.ChargePoint> _chargePoints;
    private readonly IQuerySession _querySession;

    public CreateChargePointCommandHandler(
        IAggregateRepository<Entities.ChargePoint> chargePoints,
        IQuerySession querySession)
    {
        _chargePoints = chargePoints;
        _querySession = querySession;
    }

    public async Task<Guid> Handle(CreateChargePointCommand command, CancellationToken cancellationToken)
    {
        var alreadyRegistered = await _querySession.Query<ChargePointReadModel>()
            .AnyAsync(cp => cp.OcppChargerId == command.OcppChargerId, cancellationToken);
        if (alreadyRegistered)
            throw new BusinessRuleValidationException(new OcppChargerIdMustBeUniqueRule(command.OcppChargerId, alreadyRegistered));

        var chargePoint = new Entities.ChargePoint(
            Guid.NewGuid(),
            command.OcppChargerId,
            command.LocationId,
            command.MaxPower,
            null);

        await _chargePoints.AddAsync(chargePoint, cancellationToken);

        return chargePoint.Id;
    }
}
