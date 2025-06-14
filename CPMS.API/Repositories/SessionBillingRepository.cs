using CPMS.API.Entities;
using CPMS.API.Projections;
using CPMS.BuildingBlocks.Infrastructure.DomainEventsDispatching;
using Marten;

namespace CPMS.API.Repositories;

public class SessionBillingRepository : ISessionBillingRepository
{
    private readonly IDocumentSession _session;
    private readonly ILogger<SessionBillingRepository> _logger;
    private readonly MartenDomainEventsAccessor _domainEventsAccessor;
    private readonly IDomainEventsDispatcher _domainEventsDispatcher;
    
    public SessionBillingRepository(IDocumentSession session,
        ILogger<SessionBillingRepository> logger,
        MartenDomainEventsAccessor domainEventsAccessor,
        IDomainEventsDispatcher domainEventsDispatcher)
    {
        _session = session;
        _logger = logger;
        _domainEventsAccessor = domainEventsAccessor;
        _domainEventsDispatcher = domainEventsDispatcher;
    }
    
    public async Task<SessionBilling?> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting SessionBilling by ID: {Id}", id);
            var billing = await _session.Events.AggregateStreamAsync<SessionBilling>(id);
            
            if (billing == null)
            {
                _logger.LogWarning("SessionBilling not found for ID: {Id}", id);
            }
            else
            {
                _logger.LogInformation("Found SessionBilling for session: {SessionId}", billing.SessionId);
            }
            
            return billing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SessionBilling by ID: {Id}", id);
            throw;
        }
    }
    
    public async Task<SessionBilling?> GetBySessionIdAsync(Guid sessionId)
    {
        var readModel = await _session.Query<SessionBillingReadModel>()
            .FirstOrDefaultAsync(b => b.SessionId == sessionId);
        
        return readModel != null ? await GetByIdAsync(readModel.Id) : null;
    }
    
    public async Task AddAsync(SessionBilling billing)
    {
        if (billing == null)
        {
            _logger.LogError("Cannot add null SessionBilling");
            throw new ArgumentNullException(nameof(billing));
        }

        if (billing.DomainEvents == null || !billing.DomainEvents.Any())
        {
            _logger.LogWarning("SessionBilling has no domain events to persist");
            return;
        }

        var domainEvents = billing.DomainEvents.ToList();
        _logger.LogInformation("Adding SessionBilling with {EventCount} domain events", billing.DomainEvents.Count);

        foreach (var domainEvent in billing.DomainEvents)
        {
            _session.Events.Append(billing.Id, domainEvent);
        }

        _domainEventsAccessor.AddEvents(domainEvents);
        billing.ClearDomainEvents();
        
        await _session.SaveChangesAsync();
        await _domainEventsDispatcher.DispatchEventsAsync();
        _logger.LogInformation("Successfully added SessionBilling: {Id}", billing.Id);
    }
    
    public async Task UpdateAsync(SessionBilling billing)
    {
        if (billing == null)
        {
            _logger.LogError("Cannot update null SessionBilling");
            throw new ArgumentNullException(nameof(billing));
        }

        if (billing.DomainEvents == null)
        {
            _logger.LogWarning("SessionBilling has null DomainEvents collection");
            return;
        }

        if (!billing.DomainEvents.Any())
        {
            _logger.LogInformation("SessionBilling has no domain events to persist");
            return;
        }
        
        var domainEvents = billing.DomainEvents.ToList();

        _logger.LogInformation("Updating SessionBilling {Id} with {EventCount} domain events", 
            billing.Id, billing.DomainEvents.Count);

        try
        {
            foreach (var domainEvent in billing.DomainEvents)
            {
                _session.Events.Append(billing.Id, domainEvent);
                _logger.LogDebug("Appended event: {EventType}", domainEvent.GetType().Name);
            }
            
            _domainEventsAccessor.AddEvents(domainEvents);
            billing.ClearDomainEvents();
            
            await _session.SaveChangesAsync();
            await _domainEventsDispatcher.DispatchEventsAsync();
            
            _logger.LogInformation("Successfully updated SessionBilling: {Id}", billing.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update SessionBilling: {Id}", billing.Id);
            throw;
        }
    }
}

public interface ISessionBillingRepository
{
    Task<SessionBilling?> GetByIdAsync(Guid id);
    Task<SessionBilling?> GetBySessionIdAsync(Guid sessionId);
    Task AddAsync(SessionBilling billing);
    Task UpdateAsync(SessionBilling billing);
}
