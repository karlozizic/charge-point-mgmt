using CPMS.API.Exceptions;
using CPMS.API.Repositories;
using MediatR;

namespace CPMS.API.Handlers.ChargeTag;

public class UpdateChargeTagExpiryCommand : IRequest
{
    public Guid Id { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class UpdateChargeTagExpiryCommandHandler : IRequestHandler<UpdateChargeTagExpiryCommand>
{
    private readonly IAggregateRepository<Entities.ChargeTag> _chargeTags;

    public UpdateChargeTagExpiryCommandHandler(IAggregateRepository<Entities.ChargeTag> chargeTags)
    {
        _chargeTags = chargeTags;
    }

    public async Task Handle(UpdateChargeTagExpiryCommand request, CancellationToken cancellationToken)
    {
        var chargeTag = await _chargeTags.LoadAsync(request.Id, cancellationToken)
                        ?? throw new NotFoundException($"ChargeTag with ID {request.Id} not found.");

        chargeTag.UpdateExpiry(request.ExpiryDate);
        await _chargeTags.SaveAsync(chargeTag, cancellationToken);
    }
}
