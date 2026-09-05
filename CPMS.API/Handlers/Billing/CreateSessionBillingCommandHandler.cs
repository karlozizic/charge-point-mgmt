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
    private readonly IAggregateRepository<Entities.SessionBilling> _billings;
    private readonly IAggregateRepository<Entities.PricingGroup> _pricingGroups;
    private readonly IQuerySession _querySession;

    public CreateSessionBillingCommandHandler(
        IAggregateRepository<Entities.SessionBilling> billings,
        IAggregateRepository<Entities.PricingGroup> pricingGroups,
        IQuerySession querySession)
    {
        _billings = billings;
        _pricingGroups = pricingGroups;
        _querySession = querySession;
    }

    public async Task<Guid> Handle(CreateSessionBillingCommand command, CancellationToken cancellationToken)
    {
        var session = await _querySession
            .Query<ChargeSessionReadModel>()
            .FirstOrDefaultAsync(s => s.Id == command.SessionId, cancellationToken);
        if (session == null)
            throw new InvalidOperationException($"Session {command.SessionId} not found");

        var chargePointId = Guid.Parse(session.ChargePointId);
        var pricingGroupReadModel = await _querySession
            .Query<PricingGroupReadModel>()
            .FirstOrDefaultAsync(pg => pg.ChargePointIds.Contains(chargePointId), cancellationToken);
        if (pricingGroupReadModel == null)
            throw new InvalidOperationException("No pricing group found for this charge point");

        var pricingGroup = await _pricingGroups.LoadAsync(pricingGroupReadModel.Id, cancellationToken);
        if (pricingGroup == null)
            throw new InvalidOperationException("Pricing group not found");

        var totalCost = pricingGroup.CalculateSessionCost(command.EnergyConsumed);

        var billing = new Entities.SessionBilling(
            Guid.NewGuid(),
            command.SessionId,
            pricingGroup.Id,
            pricingGroup.BasePrice,
            totalCost - pricingGroup.BasePrice,
            pricingGroup.Currency);

        await _billings.SaveAsync(billing, cancellationToken);
        return billing.Id;
    }
}
