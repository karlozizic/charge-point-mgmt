using CPMS.API.Projections;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeSession;

public class AuthorizeTagCommand : IRequest<bool>
{
    public string TagId { get; set; }
}

public class AuthorizeTagCommandHandler : IRequestHandler<AuthorizeTagCommand, bool>
{
    private readonly IQuerySession _querySession;

    public AuthorizeTagCommandHandler(IQuerySession querySession)
    {
        _querySession = querySession;
    }
    
    public async Task<bool> Handle(AuthorizeTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _querySession
            .Query<ChargeTagReadModel>()
            .FirstOrDefaultAsync(t => t.TagId == request.TagId, cancellationToken);
                
        return tag != null && !tag.Blocked && 
               (!tag.ExpiryDate.HasValue || tag.ExpiryDate.Value > DateTime.UtcNow);
    }
}