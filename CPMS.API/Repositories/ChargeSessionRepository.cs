using CPMS.API.Entities;
using CPMS.API.Projections;
using Marten;
using MediatR;

namespace CPMS.API.Repositories;

public class ChargeSessionRepository : IChargeSessionRepository
{
    private readonly IDocumentSession _session;
    private readonly IMediator _mediator;
    
    public ChargeSessionRepository(IDocumentSession session,
        IMediator mediator)
    {
        _session = session;
        _mediator = mediator;
    }
    
    public async Task<ChargeSession?> GetByIdAsync(Guid id)
    {
        return await _session.Events.AggregateStreamAsync<ChargeSession>(id);
    }

    public async Task<ChargeSession?> GetByTransactionIdAsync(int transactionId)
    {
        var readModel = await _session.Query<ChargeSessionReadModel>()
            .FirstOrDefaultAsync(cs => cs.TransactionId == transactionId);
                
        if (readModel == null)
            return null;
                
        return await GetByIdAsync(readModel.Id);
    }

    public async Task AddAsync(ChargeSession chargeSession)
    {
        foreach (var domainEvent in chargeSession.DomainEvents)
        {
            _session.Events.Append(chargeSession.Id, domainEvent);
        }
            
        chargeSession.ClearDomainEvents();
            
        await _session.SaveChangesAsync();
    }

    public async Task UpdateAsync(ChargeSession chargeSession)
    {
        foreach (var domainEvent in chargeSession.DomainEvents)
        {
            _session.Events.Append(chargeSession.Id, domainEvent);
            await _mediator.Publish(domainEvent);
        }
            
        chargeSession.ClearDomainEvents();
            
        await _session.SaveChangesAsync();
    }
}

public interface IChargeSessionRepository
{
    Task<ChargeSession?> GetByIdAsync(Guid id);
    Task<ChargeSession?> GetByTransactionIdAsync(int transactionId);
    Task AddAsync(ChargeSession chargeSession);
    Task UpdateAsync(ChargeSession chargeSession);
}