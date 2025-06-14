using CPMS.API.Entities;
using CPMS.BuildingBlocks.Infrastructure.DomainEventsDispatching;
using Marten;

namespace CPMS.API.Repositories;

public class PricingGroupRepository : IPricingGroupRepository
{
    private readonly IDocumentSession _session;
    private readonly MartenDomainEventsAccessor _domainEventsAccessor;
    private readonly IDomainEventsDispatcher _domainEventsDispatcher;

    public PricingGroupRepository(IDocumentSession session,
        MartenDomainEventsAccessor domainEventsAccessor,
        IDomainEventsDispatcher domainEventsDispatcher)
    {
        _session = session;
        _domainEventsAccessor = domainEventsAccessor;
        _domainEventsDispatcher = domainEventsDispatcher;
    }
    
    public async Task<PricingGroup?> GetByIdAsync(Guid id)
    {
        return await _session.Events.AggregateStreamAsync<PricingGroup>(id);
    }
    
    public async Task AddAsync(PricingGroup pricingGroup)
    {
        var domainEvents = pricingGroup.DomainEvents.ToList();
        
        foreach (var domainEvent in pricingGroup.DomainEvents)
        {
            _session.Events.Append(pricingGroup.Id, domainEvent);
        }
        
        _domainEventsAccessor.AddEvents(domainEvents);
        pricingGroup.ClearDomainEvents();
        
        await _session.SaveChangesAsync();
        await _domainEventsDispatcher.DispatchEventsAsync();
    }
    
    public async Task UpdateAsync(PricingGroup pricingGroup)
    {
        var domainEvents = pricingGroup.DomainEvents.ToList();
        
        foreach (var domainEvent in pricingGroup.DomainEvents)
        {
            _session.Events.Append(pricingGroup.Id, domainEvent);
        }
        
        _domainEventsAccessor.AddEvents(domainEvents);
        pricingGroup.ClearDomainEvents();
        
        await _session.SaveChangesAsync();
        await _domainEventsDispatcher.DispatchEventsAsync();
    }
}

public interface IPricingGroupRepository
{
    Task<PricingGroup?> GetByIdAsync(Guid id);
    Task AddAsync(PricingGroup pricingGroup);
    Task UpdateAsync(PricingGroup pricingGroup);
}

