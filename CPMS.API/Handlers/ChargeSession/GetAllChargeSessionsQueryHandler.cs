using CPMS.API.Projections;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeSession;

public class GetAllChargeSessionsQuery : IRequest<IReadOnlyList<ChargeSessionReadModel>>
{
    public int? PageNumber { get; set; }
    public int? PageSize { get; set; }
    public SessionStatus? Status { get; set; }
    public string? TagId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}


public class GetAllChargeSessionsQueryHandler : IRequestHandler<GetAllChargeSessionsQuery, IReadOnlyList<ChargeSessionReadModel>>
{
    private readonly IQuerySession _querySession;

    public GetAllChargeSessionsQueryHandler(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public async Task<IReadOnlyList<ChargeSessionReadModel>> Handle(GetAllChargeSessionsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<ChargeSessionReadModel> query = _querySession.Query<ChargeSessionReadModel>();

        if (request.Status.HasValue)
        {
            query = query.Where(s => s.Status == request.Status.ToString());
        }

        if (!string.IsNullOrEmpty(request.TagId))
            query = query.Where(s => s.TagId == request.TagId);

        if (request.StartDate.HasValue)
            query = query.Where(s => s.StartTime >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(s => s.StartTime <= request.EndDate.Value);

        if (request.PageNumber.HasValue && request.PageSize.HasValue)
            query = query.Skip((request.PageNumber.Value - 1) * request.PageSize.Value)
                .Take(request.PageSize.Value);

        return await query.OrderByDescending(s => s.StartTime)
            .ToListAsync(cancellationToken);
    }
}