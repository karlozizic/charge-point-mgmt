using CPMS.API.Exceptions;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeTag;

public class UnblockChargeTagCommand : IRequest
{
    public Guid Id { get; set; }
}

public class UnblockChargeTagCommandHandler : IRequestHandler<UnblockChargeTagCommand>
{
    private readonly IDocumentSession _documentSession;

    public UnblockChargeTagCommandHandler(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    public async Task Handle(UnblockChargeTagCommand request, CancellationToken cancellationToken)
    {
        var chargeTag = await _documentSession.Events.AggregateStreamAsync<Entities.ChargeTag>(request.Id, token: cancellationToken);

        if (chargeTag == null)
            throw new NotFoundException($"ChargeTag with ID {request.Id} not found.");

        chargeTag.Unblock();

        foreach (var @event in chargeTag.DomainEvents)
            _documentSession.Events.Append(request.Id, @event);

        await _documentSession.SaveChangesAsync(cancellationToken);
    }
}