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

/// <summary>
/// Materialises every matching session and aggregates in memory. Known bottleneck; the aggregation
/// belongs in SQL.
/// </summary>
public class GetChargeSessionStatsQueryHandler : IRequestHandler<GetChargeSessionStatsQuery, ChargeSessionStatsDto>
{
    private readonly IQuerySession _querySession;

    public GetChargeSessionStatsQueryHandler(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public async Task<ChargeSessionStatsDto> Handle(GetChargeSessionStatsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<ChargeSessionReadModel> query = _querySession.Query<ChargeSessionReadModel>();

        if (request.FromDate.HasValue)
            query = query.Where(s => s.StartTime >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(s => s.StartTime <= request.ToDate.Value);

        if (!string.IsNullOrEmpty(request.ChargePointId))
            query = query.Where(s => s.ChargePointId == request.ChargePointId);

        var sessions = await query.ToListAsync(cancellationToken);

        var completed = sessions.Where(s => s.StopTime.HasValue).ToList();
        var withEnergy = sessions.Where(s => s.EnergyDeliveredKWh > 0).ToList();

        return new ChargeSessionStatsDto
        {
            TotalSessions = sessions.Count,
            ActiveSessions = sessions.Count(s => s.Status == nameof(SessionStatus.Started)),
            CompletedSessions = sessions.Count(s => s.Status == nameof(SessionStatus.Stopped)),
            TotalEnergyDelivered = sessions.Sum(s => s.EnergyDeliveredKWh ?? 0),
            AverageSessionDuration = completed.Count == 0
                ? 0
                : completed.Average(s => (s.StopTime!.Value - s.StartTime).TotalMinutes),
            AverageEnergyPerSession = withEnergy.Count == 0
                ? 0
                : withEnergy.Average(s => s.EnergyDeliveredKWh!.Value)
        };
    }
}
