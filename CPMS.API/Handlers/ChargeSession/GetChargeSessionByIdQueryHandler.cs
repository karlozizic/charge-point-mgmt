using CPMS.API.Dtos;
using CPMS.API.Projections;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeSession;

public class GetChargeSessionByIdQuery : IRequest<ChargeSessionDetailDto?>
{
    public Guid SessionId { get; set; }
}

public class GetChargeSessionByIdQueryHandler : IRequestHandler<GetChargeSessionByIdQuery, ChargeSessionDetailDto?>
{
    private readonly IQuerySession _querySession;

    public GetChargeSessionByIdQueryHandler(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public async Task<ChargeSessionDetailDto?> Handle(GetChargeSessionByIdQuery request, CancellationToken cancellationToken)
    {
        var session = await _querySession.Query<ChargeSessionReadModel>()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session == null) return null;

        var readings = await _querySession.Query<MeterValueReadModel>()
            .Where(m => m.SessionId == request.SessionId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync(cancellationToken);

        return new ChargeSessionDetailDto
        {
            Id = session.Id,
            TransactionId = session.TransactionId,
            ChargePointId = session.ChargePointId,
            ConnectorId = session.ConnectorId,
            TagId = session.TagId,
            TagName = session.TagName,
            StartTime = session.StartTime,
            StopTime = session.StopTime,
            StartMeterValue = session.StartMeterValue,
            StopMeterValue = session.StopMeterValue,
            EnergyDeliveredKWh = session.EnergyDeliveredKWh,
            Status = session.Status,
            StopReason = session.StopReason,
            MeterValues = readings.ToList()
        };
    }
}
