using CPMS.API.Exceptions;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeTag;

public class UpdateChargeTagCommand : IRequest
{
    public Guid Id { get; set; }
    public string TagId { get; set; }
}

public class UpdateChargeTagCommandHandler : IRequestHandler<UpdateChargeTagCommand>
{
    private readonly IDocumentSession _documentSession;

    public UpdateChargeTagCommandHandler(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    public async Task Handle(UpdateChargeTagCommand request, CancellationToken cancellationToken)
    {
        var chargeTag = await _documentSession.Events.AggregateStreamAsync<Entities.ChargeTag>(request.Id, token: cancellationToken);
        
        if (chargeTag == null)
            throw new NotFoundException($"ChargeTag with ID {request.Id} not found.");

        chargeTag.UpdateTagId(request.TagId);

        foreach (var @event in chargeTag.DomainEvents)
        {
            _documentSession.Events.Append(request.Id, @event);
        }

        await _documentSession.SaveChangesAsync(cancellationToken);
    }
}