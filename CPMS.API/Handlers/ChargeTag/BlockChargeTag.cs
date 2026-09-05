using CPMS.API.Exceptions;
using CPMS.API.Repositories;
using MediatR;

namespace CPMS.API.Handlers.ChargeTag;

public class BlockChargeTagCommand : IRequest
{
    public Guid Id { get; set; }
}

public class BlockChargeTagCommandHandler : IRequestHandler<BlockChargeTagCommand>
{
    private readonly IAggregateRepository<Entities.ChargeTag> _chargeTags;

    public BlockChargeTagCommandHandler(IAggregateRepository<Entities.ChargeTag> chargeTags)
    {
        _chargeTags = chargeTags;
    }

    public async Task Handle(BlockChargeTagCommand request, CancellationToken cancellationToken)
    {
        var chargeTag = await _chargeTags.LoadAsync(request.Id, cancellationToken)
                        ?? throw new NotFoundException($"ChargeTag with ID {request.Id} not found.");

        chargeTag.Block();
        await _chargeTags.SaveAsync(chargeTag, cancellationToken);
    }
}
