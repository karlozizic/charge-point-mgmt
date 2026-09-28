using CPMS.API.Dtos;
using CPMS.API.Projections;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeSession;

public class GetChargeSessionStatsQuery : IRequest<ChargeSessionStatsDto>
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? ChargePointId { get; set; }
}

public class GetChargeSessionStatsQueryHandler : IRequestHandler<GetChargeSessionStatsQuery, ChargeSessionStatsDto>
{
    private readonly IQuerySession _querySession;

    public GetChargeSessionStatsQueryHandler(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public async Task<ChargeSessionStatsDto> Handle(GetChargeSessionStatsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<ChargeSessionReadModel> sessions = _querySession.Query<ChargeSessionReadModel>();

        if (request.FromDate.HasValue)
        {
            var from = AsStored(request.FromDate.Value);
            sessions = sessions.Where(s => s.StartTime >= from);
        }

        if (request.ToDate.HasValue)
        {
            var to = AsStored(request.ToDate.Value);
            sessions = sessions.Where(s => s.StartTime <= to);
        }

        if (!string.IsNullOrEmpty(request.ChargePointId))
            sessions = sessions.Where(s => s.ChargePointId == request.ChargePointId);

        var completed = sessions.Where(s => s.DurationMinutes != null);
        var withEnergy = sessions.Where(s => s.EnergyDeliveredKWh > 0);

        return new ChargeSessionStatsDto
        {
            TotalSessions = await sessions.CountAsync(cancellationToken),
            ActiveSessions = await sessions.CountAsync(s => s.Status == nameof(SessionStatus.Started), cancellationToken),
            CompletedSessions = await sessions.CountAsync(s => s.Status == nameof(SessionStatus.Stopped), cancellationToken),
            TotalEnergyDelivered = await withEnergy.AnyAsync(cancellationToken)
                ? await withEnergy.SumAsync(s => s.EnergyDeliveredKWh!.Value, cancellationToken)
                : 0,
            AverageSessionDuration = await completed.AnyAsync(cancellationToken)
                ? await completed.AverageAsync(s => s.DurationMinutes!.Value, cancellationToken)
                : 0,
            AverageEnergyPerSession = await withEnergy.AnyAsync(cancellationToken)
                ? await withEnergy.AverageAsync(s => s.EnergyDeliveredKWh!.Value, cancellationToken)
                : 0
        };
    }

    private static DateTime AsStored(DateTime value) =>
        DateTime.SpecifyKind(value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : value, DateTimeKind.Unspecified);
}
