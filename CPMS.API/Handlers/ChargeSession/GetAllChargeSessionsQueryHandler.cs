using CPMS.API.Projections;
using CPMS.Core.Common;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeSession;

public class GetAllChargeSessionsQuery : IRequest<PagedResult<ChargeSessionReadModel>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public SessionStatus? Status { get; set; }
    public string? TagId { get; set; }
    public string? ChargePointId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}


public class GetAllChargeSessionsQueryHandler : IRequestHandler<GetAllChargeSessionsQuery, PagedResult<ChargeSessionReadModel>>
{
    private readonly IQuerySession _querySession;

    public GetAllChargeSessionsQueryHandler(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public async Task<PagedResult<ChargeSessionReadModel>> Handle(GetAllChargeSessionsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<ChargeSessionReadModel> query = _querySession.Query<ChargeSessionReadModel>();

        if (request.Status.HasValue)
        {
            query = query.Where(s => s.Status == request.Status.ToString());
        }

        if (!string.IsNullOrEmpty(request.TagId))
        {
            query = query.Where(s => s.TagId == request.TagId);
        }

        if (!string.IsNullOrEmpty(request.ChargePointId))
        {
            query = query.Where(s => s.ChargePointId == request.ChargePointId);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(s => s.StartTime >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(s => s.StartTime <= request.EndDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var sessions = await query
            .OrderByDescending(s => s.StartTime)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ChargeSessionReadModel>(sessions, totalCount, request.PageNumber, request.PageSize);
    }
}