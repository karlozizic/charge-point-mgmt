using CPMS.API.Entities;
using CPMS.API.Projections;
using CPMS.BuildingBlocks.Infrastructure.DomainEventsDispatching;
using Marten;

namespace CPMS.API.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly IDocumentSession _session;
    private readonly IDomainEventsDispatcher _domainEventsDispatcher;
    private readonly MartenDomainEventsAccessor _eventsAccessor;

    public LocationRepository(IDocumentSession session,
        IDomainEventsDispatcher domainEventsDispatcher,
        MartenDomainEventsAccessor eventsAccessor)
    {
        _session = session;
        _domainEventsDispatcher = domainEventsDispatcher;
        _eventsAccessor = eventsAccessor;
    }
    
    public async Task<Location?> GetByIdAsync(Guid id)
    {
        return await _session.Events.AggregateStreamAsync<Location>(id);
    }

    public async Task<Location?> GetByNameAsync(string name)
    {
        var readModel = await _session.Query<LocationReadModel>()
            .FirstOrDefaultAsync(l => l.Name == name);
        
        if (readModel == null)
            return null;
            
        return await GetByIdAsync(readModel.Id);
    }

    public async Task AddAsync(Location location)
    {
        var events = location.DomainEvents.ToList();
        
        foreach (var domainEvent in location.DomainEvents)
        {
            _session.Events.Append(location.Id, domainEvent);
        }
        
        _eventsAccessor.AddEvents(events);    
        location.ClearDomainEvents();
                
        await _session.SaveChangesAsync();
        await _domainEventsDispatcher.DispatchEventsAsync();
    }

    public async Task UpdateAsync(Location location)
    {
        var events = location.DomainEvents.ToList();
        
        foreach (var domainEvent in location.DomainEvents)
        {
            _session.Events.Append(location.Id, domainEvent);
        }
        
        _eventsAccessor.AddEvents(events);
        location.ClearDomainEvents();
            
        await _session.SaveChangesAsync();
        await _domainEventsDispatcher.DispatchEventsAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var events = await _session.Events.FetchStreamAsync(id);
        return events.Any();
    }
}

public interface ILocationRepository
{
    Task<Location?> GetByIdAsync(Guid id);
    Task<Location?> GetByNameAsync(string name);
    Task AddAsync(Location location);
    Task UpdateAsync(Location location);
    Task<bool> ExistsAsync(Guid id);
}