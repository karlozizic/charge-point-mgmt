using CPMS.API.Projections;
using CPMS.API.Repositories;
using Marten;
using MediatR;

namespace CPMS.API.Handlers.Billing;

public class CreateSessionBillingCommand : IRequest<Guid>
{
    public Guid SessionId { get; set; }
    public double EnergyConsumed { get; set; }
}


public class CreateSessionBillingCommandHandler : IRequestHandler<CreateSessionBillingCommand, Guid>
{
    private readonly ISessionBillingRepository _billingRepository;
    private readonly IPricingGroupRepository _pricingRepository;
    private readonly IQuerySession _querySession;
    
    public CreateSessionBillingCommandHandler(
        ISessionBillingRepository billingRepository,
        IPricingGroupRepository pricingRepository,
        IQuerySession querySession)
    {
        _billingRepository = billingRepository;
        _pricingRepository = pricingRepository;
        _querySession = querySession;
    }
    
    public async Task<Guid> Handle(CreateSessionBillingCommand command, CancellationToken cancellationToken)
    {
        var sessionReadModel = await _querySession
            .Query<ChargeSessionReadModel>()
            .FirstOrDefaultAsync(s => s.Id == command.SessionId, cancellationToken);
            
        if (sessionReadModel == null)
            throw new InvalidOperationException($"Session {command.SessionId} not found");
        
        var pricingGroupReadModel = await _querySession
            .Query<PricingGroupReadModel>()
            .FirstOrDefaultAsync(pg => pg.ChargePointIds.Contains(Guid.Parse(sessionReadModel.ChargePointId)), 
                cancellationToken);
        
        if (pricingGroupReadModel == null)
            throw new InvalidOperationException("No pricing group found for this charge point");
        
        var pricingGroup = await _pricingRepository.GetByIdAsync(pricingGroupReadModel.Id);
        if (pricingGroup == null)
            throw new InvalidOperationException("Pricing group not found");
        
        var totalCost = pricingGroup.CalculateSessionCost(command.EnergyConsumed);
        var baseAmount = pricingGroup.BasePrice;
        var energyAmount = totalCost - baseAmount;
        
        var billing = new Entities.SessionBilling(
            command.SessionId,
            pricingGroup.Id,
            baseAmount,
            energyAmount,
            pricingGroup.Currency);
        
        await _billingRepository.AddAsync(billing);
        return billing.Id;
    }
}