using CPMS.API.Exceptions;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeTag;

public class UpdateChargeTagExpiryCommand : IRequest
{
    public Guid Id { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class UpdateChargeTagExpiryCommandHandler : IRequestHandler<UpdateChargeTagExpiryCommand>
{
    private readonly IDocumentSession _documentSession;

    public UpdateChargeTagExpiryCommandHandler(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    public async Task Handle(UpdateChargeTagExpiryCommand request, CancellationToken cancellationToken)
    {
        var chargeTag = await _documentSession.Events.AggregateStreamAsync<Entities.ChargeTag>(request.Id, token: cancellationToken);

        if (chargeTag == null)
            throw new NotFoundException($"ChargeTag with ID {request.Id} not found.");

        chargeTag.UpdateExpiry(request.ExpiryDate);

        foreach (var @event in chargeTag.DomainEvents)
        {
            _documentSession.Events.Append(request.Id, @event);
        }

        await _documentSession.SaveChangesAsync(cancellationToken);
    }
}