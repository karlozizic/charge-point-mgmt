using CPMS.API.Projections;
using CPMS.Core.Models.Responses;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeSession;

public class AuthorizeTagCommand : IRequest<AuthorizationStatus>
{
    public string TagId { get; set; }
}

public class AuthorizeTagCommandHandler : IRequestHandler<AuthorizeTagCommand, AuthorizationStatus>
{
    private readonly IQuerySession _querySession;

    public AuthorizeTagCommandHandler(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public async Task<AuthorizationStatus> Handle(AuthorizeTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _querySession
            .Query<ChargeTagReadModel>()
            .FirstOrDefaultAsync(t => t.TagId == request.TagId, cancellationToken);

        if (tag == null)
            return AuthorizationStatus.Invalid;

        if (tag.Blocked)
            return AuthorizationStatus.Blocked;

        if (tag.ExpiryDate.HasValue && tag.ExpiryDate.Value <= DateTime.UtcNow)
            return AuthorizationStatus.Expired;

        return AuthorizationStatus.Accepted;
    }
}
