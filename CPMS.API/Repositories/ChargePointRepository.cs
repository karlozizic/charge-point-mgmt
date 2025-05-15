using CPMS.API.Entities;
using CPMS.API.Projections;
using Marten;

namespace CPMS.API.Repositories;

public class ChargePointRepository : IChargePointRepository
{
    private readonly IDocumentSession _session;

    public ChargePointRepository(IDocumentSession session)
    {
        _session = session;
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
        foreach (var domainEvent in chargePoint.DomainEvents)
        {
            _session.Events.Append(chargePoint.Id, domainEvent);
        }
            
        chargePoint.ClearDomainEvents();
            
        await _session.SaveChangesAsync();
    }
        
    public async Task UpdateAsync(ChargePoint chargePoint)
    {
        foreach (var domainEvent in chargePoint.DomainEvents)
        {
            _session.Events.Append(chargePoint.Id, domainEvent);
        }
            
        chargePoint.ClearDomainEvents();
            
        await _session.SaveChangesAsync();
    }
}

public interface IChargePointRepository
{
    Task<ChargePoint?> GetByOcppChargerIdAsync(string ocppChargerId);
    Task<ChargePoint?> GetByIdAsync(Guid id);
    Task AddAsync(ChargePoint chargePoint);
    Task UpdateAsync(ChargePoint chargePoint);
}