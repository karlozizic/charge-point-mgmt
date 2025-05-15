using Marten;
using MediatR;

namespace CPMS.API.Handlers.ChargeTag;

public class UpdateChargeTagCommand : IRequest
{
    public Guid Id { get; set; }
    public string TagName { get; set; }
    public string ParentTagId { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool? Blocked { get; set; }
}

public class UpdateChargeTagCommandHandler : IRequestHandler<UpdateChargeTagCommand>
{
    private readonly IDocumentSession _documentSession;
    private readonly IMediator _mediator;

    public UpdateChargeTagCommandHandler(IDocumentSession documentSession, IMediator mediator)
    {
        _documentSession = documentSession;
        _mediator = mediator;
    }

    public async Task Handle(UpdateChargeTagCommand request, CancellationToken cancellationToken)
    {
        var chargeTag = await _documentSession.Events.AggregateStreamAsync<Entities.ChargeTag>(request.Id, token: cancellationToken);

        if (chargeTag == null)
        {
            throw new InvalidOperationException($"Tag s ID {request.Id} ne postoji.");
        }

        if (request.ExpiryDate.HasValue)
        {
            await _mediator.Send(new UpdateChargeTagExpiryCommand { Id = request.Id, ExpiryDate = request.ExpiryDate });
        }

        if (request.Blocked.HasValue)
        {
            if (request.Blocked.Value && !chargeTag.Blocked)
            {
                await _mediator.Send(new BlockChargeTagCommand { Id = request.Id });
            }
            else if (!request.Blocked.Value && chargeTag.Blocked)
            {
                await _mediator.Send(new UnblockChargeTagCommand { Id = request.Id });
            }
        }
    }
}