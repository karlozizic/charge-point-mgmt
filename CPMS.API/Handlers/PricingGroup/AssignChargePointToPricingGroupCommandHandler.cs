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
    private readonly IPricingGroupRepository _repository;

    public AssignChargePointToPricingGroupCommandHandler(IPricingGroupRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(AssignChargePointToPricingGroupCommand command, CancellationToken cancellationToken)
    {
        var pricingGroup = await _repository.GetByIdAsync(command.PricingGroupId);
        
        if (pricingGroup == null)
            throw new InvalidOperationException($"Pricing group {command.PricingGroupId} not found");

        pricingGroup.AssignChargePoint(command.ChargePointId);
        await _repository.UpdateAsync(pricingGroup);
    }
}