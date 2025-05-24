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
            IQueryable<ChargeSessionReadModel> query = _querySession.Query<ChargeSessionReadModel>();

            if (request.FromDate.HasValue)
            {
                query = query.Where(s => s.StartTime >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                query = query.Where(s => s.StartTime <= request.ToDate.Value);
            }

            if (!string.IsNullOrEmpty(request.ChargePointId))
            {
                query = query.Where(s => s.ChargePointId == request.ChargePointId);
            }

            var sessions = await query.ToListAsync(cancellationToken);

            return new ChargeSessionStatsDto
            {
                TotalSessions = sessions.Count,
                ActiveSessions = sessions.Count(s => s.Status == nameof(SessionStatus.Started)),
                CompletedSessions = sessions.Count(s => s.Status == nameof(SessionStatus.Stopped)),
                TotalEnergyDelivered = sessions.Where(s => s.EnergyDeliveredKWh.HasValue)
                                              .Sum(s => s.EnergyDeliveredKWh.Value),
                AverageSessionDuration = sessions.Where(s => s.StopTime.HasValue)
                                               .Select(s => s.StopTime!.Value - s.StartTime)
                                               .DefaultIfEmpty()
                                               .Average(ts => ts.TotalMinutes),
                AverageEnergyPerSession = sessions.Where(s => s.EnergyDeliveredKWh.HasValue && s.EnergyDeliveredKWh > 0)
                                                 .DefaultIfEmpty()
                                                 .Average(s => s.EnergyDeliveredKWh ?? 0)
            };
        }
    }