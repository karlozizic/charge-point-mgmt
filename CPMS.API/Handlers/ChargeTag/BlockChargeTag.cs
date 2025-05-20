using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeTag;

public class BlockChargeTagCommand : IRequest
{
    public Guid Id { get; set; }
}

public class BlockChargeTagCommandHandler : IRequestHandler<BlockChargeTagCommand>
{
    private readonly IDocumentSession _documentSession;

    public BlockChargeTagCommandHandler(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    public async Task Handle(BlockChargeTagCommand request, CancellationToken cancellationToken)
    {
        var chargeTag = await _documentSession.Events.AggregateStreamAsync<Entities.ChargeTag>(request.Id, token: cancellationToken);

        if (chargeTag == null)
            throw new InvalidOperationException($"ChargeTag with ID {request.Id} not found.");

        chargeTag.Block();

        foreach (var @event in chargeTag.DomainEvents)
        {
            _documentSession.Events.Append(request.Id, @event);
        }

        await _documentSession.SaveChangesAsync(cancellationToken);
    }
}