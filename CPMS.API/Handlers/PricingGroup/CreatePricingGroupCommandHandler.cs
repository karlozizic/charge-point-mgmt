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
    private readonly IPricingGroupRepository _repository;
    
    public CreatePricingGroupCommandHandler(IPricingGroupRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<Guid> Handle(CreatePricingGroupCommand command, CancellationToken cancellationToken)
    {
        var pricingGroupId = Guid.NewGuid();
        
        var pricingGroup = new Entities.PricingGroup(
            pricingGroupId,
            command.Name,
            command.BasePrice,
            command.PricePerKwh,
            command.Currency);
        
        await _repository.AddAsync(pricingGroup);
        return pricingGroupId;
    }
}