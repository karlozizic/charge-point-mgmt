using CPMS.API.Projections;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeTag;

public class GetChargeTagByIdQuery : IRequest<ChargeTagReadModel>
{
    public Guid Id { get; set; }
}

public class GetChargeTagByIdQueryHandler : IRequestHandler<GetChargeTagByIdQuery, ChargeTagReadModel>
{
    private readonly IQuerySession _querySession;

    public GetChargeTagByIdQueryHandler(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public async Task<ChargeTagReadModel> Handle(GetChargeTagByIdQuery request, CancellationToken cancellationToken)
    {
        return await _querySession.Query<ChargeTagReadModel>()
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
    }
}