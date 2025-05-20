using CPMS.API.Projections;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeTag;

public class CreateChargeTagCommand : IRequest<Guid>
{
    public string TagId { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class CreateChargeTagCommandHandler : IRequestHandler<CreateChargeTagCommand, Guid>
{
    private readonly IDocumentSession _documentSession;

    public CreateChargeTagCommandHandler(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    public async Task<Guid> Handle(CreateChargeTagCommand request, CancellationToken cancellationToken)
    {
        var existingTag = await _documentSession
            .Query<ChargeTagReadModel>()
            .FirstOrDefaultAsync(t => t.TagId == request.TagId, cancellationToken);

        if (existingTag != null)
            throw new InvalidOperationException($"Tag with ID {request.TagId} already exists.");

        var tagId = Guid.NewGuid();
        var chargeTag = new Entities.ChargeTag(
            tagId,
            request.TagId,
            request.ExpiryDate
        );

        foreach (var @event in chargeTag.DomainEvents)
        {
            _documentSession.Events.Append(tagId, @event);
        }

        await _documentSession.SaveChangesAsync(cancellationToken);

        return tagId;
    }
}