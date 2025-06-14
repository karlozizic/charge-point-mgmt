using CPMS.BuildingBlocks.Domain;

namespace CPMS.BuildingBlocks.Infrastructure.DomainEventsDispatching;

public interface IDomainEventsAccessor
{
    void AddEvents(IEnumerable<IDomainEvent> domainEvents);
    IReadOnlyCollection<IDomainEvent> GetAllEvents();

    void ClearAllEvents();
}