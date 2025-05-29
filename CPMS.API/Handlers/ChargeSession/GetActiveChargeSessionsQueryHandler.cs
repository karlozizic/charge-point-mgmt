using CPMS.API.Projections;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeSession;

public class GetActiveChargeSessionsQuery : IRequest<IReadOnlyList<ChargeSessionReadModel>>
{
}

public class GetActiveChargeSessionsQueryHandler : IRequestHandler<GetActiveChargeSessionsQuery, IReadOnlyList<ChargeSessionReadModel>>
{
    private readonly IQuerySession _querySession;

    public GetActiveChargeSessionsQueryHandler(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public async Task<IReadOnlyList<ChargeSessionReadModel>> Handle(GetActiveChargeSessionsQuery request, CancellationToken cancellationToken)
    {
        return await _querySession.Query<ChargeSessionReadModel>()
            .Where(s => s.Status == nameof(SessionStatus.Started))
            .OrderByDescending(s => s.StartTime)
            .ToListAsync(cancellationToken);
    }
}