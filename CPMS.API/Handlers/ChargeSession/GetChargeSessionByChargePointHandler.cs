using CPMS.API.Projections;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeSession;

public class GetChargeSessionsByChargePointQuery : IRequest<IReadOnlyList<ChargeSessionReadModel>>
{
    public string ChargePointId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class GetChargeSessionsByChargePointQueryHandler : IRequestHandler<GetChargeSessionsByChargePointQuery, IReadOnlyList<ChargeSessionReadModel>>
{
    private readonly IQuerySession _querySession;

    public GetChargeSessionsByChargePointQueryHandler(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public async Task<IReadOnlyList<ChargeSessionReadModel>> Handle(GetChargeSessionsByChargePointQuery request, CancellationToken cancellationToken)
    {
        var query = _querySession.Query<ChargeSessionReadModel>()
            .Where(s => s.ChargePointId == request.ChargePointId);

        if (request.FromDate.HasValue)
        {
            query = query.Where(s => s.StartTime >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(s => s.StartTime <= request.ToDate.Value);
        }

        return await query.OrderByDescending(s => s.StartTime).ToListAsync(cancellationToken);
    }
}