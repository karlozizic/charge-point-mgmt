using CPMS.API.Exceptions;
using CPMS.API.Repositories;
using MediatR;

namespace CPMS.API.Handlers.ChargeTag;

public class UpdateChargeTagCommand : IRequest
{
    public Guid Id { get; set; }
    public string TagId { get; set; }
}

public class UpdateChargeTagCommandHandler : IRequestHandler<UpdateChargeTagCommand>
{
    private readonly IAggregateRepository<Entities.ChargeTag> _chargeTags;

    public UpdateChargeTagCommandHandler(IAggregateRepository<Entities.ChargeTag> chargeTags)
    {
        _chargeTags = chargeTags;
    }

    public async Task Handle(UpdateChargeTagCommand request, CancellationToken cancellationToken)
    {
        var chargeTag = await _chargeTags.LoadAsync(request.Id, cancellationToken)
                        ?? throw new NotFoundException($"ChargeTag with ID {request.Id} not found.");

        chargeTag.UpdateTagId(request.TagId);
        await _chargeTags.SaveAsync(chargeTag, cancellationToken);
    }
}
