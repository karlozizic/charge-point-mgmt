using CPMS.API.Repositories;
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

    public CreateChargePointCommandHandler(IAggregateRepository<Entities.ChargePoint> chargePoints)
    {
        _chargePoints = chargePoints;
    }

    public async Task<Guid> Handle(CreateChargePointCommand command, CancellationToken cancellationToken)
    {
        var chargePoint = new Entities.ChargePoint(
            Guid.NewGuid(),
            command.OcppChargerId,
            command.LocationId,
            command.MaxPower,
            null);

        await _chargePoints.SaveAsync(chargePoint, cancellationToken);

        return chargePoint.Id;
    }
}
