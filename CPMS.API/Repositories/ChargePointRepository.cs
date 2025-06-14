using CPMS.API.Entities;
using CPMS.API.Projections;
using CPMS.BuildingBlocks.Infrastructure.DomainEventsDispatching;
using Marten;

namespace CPMS.API.Repositories;

public class ChargePointRepository : IChargePointRepository
{
    private readonly IDocumentSession _session;
    private readonly MartenDomainEventsAccessor _domainEventsAccessor;
    private readonly IDomainEventsDispatcher _domainEventsDispatcher;
    
    public ChargePointRepository(IDocumentSession session,
        MartenDomainEventsAccessor domainEventsAccessor,
        IDomainEventsDispatcher domainEventsDispatcher)
        {
            _session = session;
            _domainEventsAccessor = domainEventsAccessor;
            _domainEventsDispatcher = domainEventsDispatcher;
        }

    public async Task<ChargePoint?> GetByIdAsync(Guid id)
    {
        return await _session.Events.AggregateStreamAsync<ChargePoint>(id);
    }

    public async Task<ChargePoint?> GetByOcppChargerIdAsync(string ocppChargerId)
    {
        var readModel = await _session.Query<ChargePointReadModel>()
            .FirstOrDefaultAsync(cp => cp.OcppChargerId == ocppChargerId);
        
        if (readModel == null)
            return null;
            
        return await GetByIdAsync(readModel.Id);
    }
        
    public async Task AddAsync(ChargePoint chargePoint)
    {
        var domainEvents = chargePoint.DomainEvents.ToList();
        
        foreach (var domainEvent in chargePoint.DomainEvents)
        {
            _session.Events.Append(chargePoint.Id, domainEvent);
        }
            
        _domainEventsAccessor.AddEvents(domainEvents);
        chargePoint.ClearDomainEvents();
            
        await _session.SaveChangesAsync();
        await _domainEventsDispatcher.DispatchEventsAsync();
    }
        
    public async Task UpdateAsync(ChargePoint chargePoint)
    {
        var domainEvents = chargePoint.DomainEvents.ToList();
        
        foreach (var domainEvent in chargePoint.DomainEvents)
        {
            _session.Events.Append(chargePoint.Id, domainEvent);
        }
            
        chargePoint.ClearDomainEvents();
        _domainEventsAccessor.AddEvents(domainEvents);
            
        await _session.SaveChangesAsync();
        await _domainEventsDispatcher.DispatchEventsAsync();
    }
}

public interface IChargePointRepository
{
    Task<ChargePoint?> GetByOcppChargerIdAsync(string ocppChargerId);
    Task<ChargePoint?> GetByIdAsync(Guid id);
    Task AddAsync(ChargePoint chargePoint);
    Task UpdateAsync(ChargePoint chargePoint);
}