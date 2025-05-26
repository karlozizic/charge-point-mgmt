using CPMS.API.Entities;
using CPMS.API.Projections;
using Marten;

namespace CPMS.API.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly IDocumentSession _session;

    public LocationRepository(IDocumentSession session)
    {
        _session = session;
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
        foreach (var domainEvent in location.DomainEvents)
        {
            _session.Events.Append(location.Id, domainEvent);
        }
            
        location.ClearDomainEvents();
            
        await _session.SaveChangesAsync();
    }

    public async Task UpdateAsync(Location location)
    {
        foreach (var domainEvent in location.DomainEvents)
        {
            _session.Events.Append(location.Id, domainEvent);
        }
            
        location.ClearDomainEvents();
            
        await _session.SaveChangesAsync();
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