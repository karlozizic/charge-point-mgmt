using CPMS.API.Repositories;
using MediatR;

namespace CPMS.API.Handlers.PricingGroup;

public class CreatePricingGroupCommand : IRequest<Guid>
{
    public string Name { get; set; }
    public decimal BasePrice { get; set; }
    public decimal PricePerKwh { get; set; }
    public string Currency { get; set; } = "EUR";
}

public class CreatePricingGroupCommandHandler : IRequestHandler<CreatePricingGroupCommand, Guid>
{
    private readonly IAggregateRepository<Entities.PricingGroup> _pricingGroups;

    public CreatePricingGroupCommandHandler(IAggregateRepository<Entities.PricingGroup> pricingGroups)
    {
        _pricingGroups = pricingGroups;
    }

    public async Task<Guid> Handle(CreatePricingGroupCommand command, CancellationToken cancellationToken)
    {
        var pricingGroup = new Entities.PricingGroup(
            Guid.NewGuid(),
            command.Name,
            command.BasePrice,
            command.PricePerKwh,
            command.Currency);

        await _pricingGroups.SaveAsync(pricingGroup, cancellationToken);
        return pricingGroup.Id;
    }
}
