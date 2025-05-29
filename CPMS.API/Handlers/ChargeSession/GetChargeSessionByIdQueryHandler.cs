using CPMS.API.Projections;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeSession;

public class GetChargeSessionByIdQuery : IRequest<ChargeSessionReadModel?>
{
    public Guid SessionId { get; set; }
}

public class GetChargeSessionByIdQueryHandler : IRequestHandler<GetChargeSessionByIdQuery, ChargeSessionReadModel?>
{
    private readonly IQuerySession _querySession;

    public GetChargeSessionByIdQueryHandler(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public async Task<ChargeSessionReadModel?> Handle(GetChargeSessionByIdQuery request, CancellationToken cancellationToken)
    {
        return await _querySession.Query<ChargeSessionReadModel>()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);
    }
}