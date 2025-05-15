using CPMS.API.Projections;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeTag;

public class GetAllChargeTagsQuery : IRequest<IReadOnlyList<ChargeTagReadModel>> 
{ 
}

public class GetAllChargeTagsQueryHandler : IRequestHandler<GetAllChargeTagsQuery, IReadOnlyList<ChargeTagReadModel>>
{
    private readonly IQuerySession _querySession;

    public GetAllChargeTagsQueryHandler(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public async Task<IReadOnlyList<ChargeTagReadModel>> Handle(GetAllChargeTagsQuery request, CancellationToken cancellationToken)
    {
        return await _querySession.Query<ChargeTagReadModel>().ToListAsync(cancellationToken);
    }
}
