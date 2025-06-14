using CPMS.API.Entities;
using CPMS.API.Projections;
using CPMS.BuildingBlocks.Infrastructure.DomainEventsDispatching;
using Marten;

namespace CPMS.API.Repositories;

public class ChargeSessionRepository : IChargeSessionRepository
{
    private readonly IDocumentSession _session;
    private readonly MartenDomainEventsAccessor _domainEventsAccessor;
    private readonly IDomainEventsDispatcher _domainEventsDispatcher;
    
    public ChargeSessionRepository(IDocumentSession session
        , MartenDomainEventsAccessor domainEventsAccessor
        , IDomainEventsDispatcher domainEventsDispatcher)
    {
        _session = session;
        _domainEventsAccessor = domainEventsAccessor;
        _domainEventsDispatcher = domainEventsDispatcher;
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
        var domainEvents = chargeSession.DomainEvents.ToList();
        
        foreach (var domainEvent in domainEvents)
        {
            _session.Events.Append(chargeSession.Id, domainEvent);
        }
            
        _domainEventsAccessor.AddEvents(domainEvents);
        chargeSession.ClearDomainEvents();
        
        await _session.SaveChangesAsync();
        await _domainEventsDispatcher.DispatchEventsAsync();
    }

    public async Task UpdateAsync(ChargeSession chargeSession)
    {
        var domainEvents = chargeSession.DomainEvents.ToList();
        
        foreach (var domainEvent in domainEvents)
        {
            _session.Events.Append(chargeSession.Id, domainEvent);
        }
        
        _domainEventsAccessor.AddEvents(domainEvents);
        chargeSession.ClearDomainEvents();
        
        await _session.SaveChangesAsync();
        await _domainEventsDispatcher.DispatchEventsAsync();
    }
}

public interface IChargeSessionRepository
{
    Task<ChargeSession?> GetByIdAsync(Guid id);
    Task<ChargeSession?> GetByTransactionIdAsync(int transactionId);
    Task AddAsync(ChargeSession chargeSession);
    Task UpdateAsync(ChargeSession chargeSession);
}