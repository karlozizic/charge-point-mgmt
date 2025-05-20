using CPMS.API.Entities;
using CPMS.API.Projections;
using Marten;

namespace CPMS.API.Repositories;

public class ChargeSessionRepository : IChargeSessionRepository
{
    private readonly IDocumentSession _session;
    
    public ChargeSessionRepository(IDocumentSession session)
    {
        _session = session;
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