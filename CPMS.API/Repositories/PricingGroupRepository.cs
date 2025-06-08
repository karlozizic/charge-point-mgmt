using CPMS.API.Entities;
using Marten;

namespace CPMS.API.Repositories;

public class PricingGroupRepository : IPricingGroupRepository
{
    private readonly IDocumentSession _session;

    public PricingGroupRepository(IDocumentSession session)
    {
        _session = session;
    }
    
    public async Task<PricingGroup?> GetByIdAsync(Guid id)
    {
        return await _session.Events.AggregateStreamAsync<PricingGroup>(id);
    }
    
    public async Task AddAsync(PricingGroup pricingGroup)
    {
        foreach (var domainEvent in pricingGroup.DomainEvents)
        {
            _session.Events.Append(pricingGroup.Id, domainEvent);
        }
        
        pricingGroup.ClearDomainEvents();
        await _session.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(PricingGroup pricingGroup)
    {
        foreach (var domainEvent in pricingGroup.DomainEvents)
        {
            _session.Events.Append(pricingGroup.Id, domainEvent);
        }
        
        pricingGroup.ClearDomainEvents();
        await _session.SaveChangesAsync();
    }
}

public interface IPricingGroupRepository
{
    Task<PricingGroup?> GetByIdAsync(Guid id);
    Task AddAsync(PricingGroup pricingGroup);
    Task UpdateAsync(PricingGroup pricingGroup);
}

