using CPMS.API.Exceptions;
using CPMS.API.Repositories;
using MediatR;

namespace CPMS.API.Handlers.PricingGroup;

public class AssignChargePointToPricingGroupCommand : IRequest
{
    public Guid PricingGroupId { get; set; }
    public Guid ChargePointId { get; set; }
}

public class AssignChargePointToPricingGroupCommandHandler : IRequestHandler<AssignChargePointToPricingGroupCommand>
{
    private readonly IAggregateRepository<Entities.PricingGroup> _pricingGroups;

    public AssignChargePointToPricingGroupCommandHandler(IAggregateRepository<Entities.PricingGroup> pricingGroups)
    {
        _pricingGroups = pricingGroups;
    }

    public async Task Handle(AssignChargePointToPricingGroupCommand command, CancellationToken cancellationToken)
    {
        var pricingGroup = await _pricingGroups.LoadAsync(command.PricingGroupId, cancellationToken);
        if (pricingGroup == null)
            throw new NotFoundException($"Pricing group {command.PricingGroupId} not found");

        pricingGroup.AssignChargePoint(command.ChargePointId);
        await _pricingGroups.SaveAsync(pricingGroup, cancellationToken);
    }
}
