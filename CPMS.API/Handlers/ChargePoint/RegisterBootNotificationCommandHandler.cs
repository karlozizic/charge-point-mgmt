using CPMS.API.Projections;
using CPMS.API.Repositories;
using CPMS.Core.Models.Requests;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargePoint;

/// <summary>Returns false when the charger is not registered; the proxy then rejects the boot.</summary>
public record BootNotificationCommand(BootNotificationRequest Request) : IRequest<bool>;

public class RegisterBootNotificationCommandHandler : IRequestHandler<BootNotificationCommand, bool>
{
    private readonly IAggregateRepository<Entities.ChargePoint> _chargePoints;
    private readonly IQuerySession _querySession;

    public RegisterBootNotificationCommandHandler(
        IAggregateRepository<Entities.ChargePoint> chargePoints,
        IQuerySession querySession)
    {
        _chargePoints = chargePoints;
        _querySession = querySession;
    }

    public async Task<bool> Handle(BootNotificationCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        var readModel = await _querySession.Query<ChargePointReadModel>()
            .FirstOrDefaultAsync(cp => cp.OcppChargerId == request.OcppChargerId, cancellationToken);
        if (readModel == null)
            return false;

        var chargePoint = await _chargePoints.LoadAsync(readModel.Id, cancellationToken);
        if (chargePoint == null)
            return false;

        chargePoint.RegisterBoot(
            request.ChargePointSerialNumber,
            request.ChargePointModel,
            request.ChargePointVendor,
            request.FirmwareVersion);

        await _chargePoints.SaveAsync(chargePoint, cancellationToken);
        return true;
    }
}
