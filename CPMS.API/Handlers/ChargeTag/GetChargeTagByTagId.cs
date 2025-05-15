using CPMS.API.Projections;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeTag;

public class GetChargeTagByTagIdQuery : IRequest<ChargeTagReadModel>
{
    public string TagId { get; set; }
}

public class GetChargeTagByTagIdQueryHandler : IRequestHandler<GetChargeTagByTagIdQuery, ChargeTagReadModel>
{
    private readonly IQuerySession _querySession;

    public GetChargeTagByTagIdQueryHandler(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public async Task<ChargeTagReadModel> Handle(GetChargeTagByTagIdQuery request, CancellationToken cancellationToken)
    {
        return await _querySession.Query<ChargeTagReadModel>()
            .FirstOrDefaultAsync(t => t.TagId == request.TagId, cancellationToken);
    }
}