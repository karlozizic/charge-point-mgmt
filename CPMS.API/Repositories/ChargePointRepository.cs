using CPMS.API.Entities;
using Marten;

namespace CPMS.API.Repositories;

public class ChargePointRepository(IDocumentSession session) : IChargePointRepository
{
    public async Task<ChargePoint?> GetByIdAsync(Guid id)
    {
        return await session.Events.AggregateStreamAsync<ChargePoint>(id);
    }
        
    public async Task AddAsync(ChargePoint chargePoint)
    {
        foreach (var domainEvent in chargePoint.DomainEvents)
        {
            session.Events.Append(chargePoint.Id, domainEvent);
        }
            
        chargePoint.ClearDomainEvents();
            
        await session.SaveChangesAsync();
    }
        
    public async Task UpdateAsync(ChargePoint chargePoint)
    {
        foreach (var domainEvent in chargePoint.DomainEvents)
        {
            session.Events.Append(chargePoint.Id, domainEvent);
        }
            
        chargePoint.ClearDomainEvents();
            
        await session.SaveChangesAsync();
    }
}

public interface IChargePointRepository
{
    Task<ChargePoint?> GetByIdAsync(Guid id);
    Task AddAsync(ChargePoint chargePoint);
    Task UpdateAsync(ChargePoint chargePoint);
}