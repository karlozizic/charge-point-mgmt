using CPMS.BuildingBlocks.Domain;

namespace CPMS.BuildingBlocks.Infrastructure.DomainEventsDispatching;


public class MartenDomainEventsAccessor : IDomainEventsAccessor
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public MartenDomainEventsAccessor()
    {
        
    }

    public void AddEvents(IEnumerable<IDomainEvent> domainEvents)
    {
        _domainEvents.AddRange(domainEvents);
    }

    public IReadOnlyCollection<IDomainEvent> GetAllEvents()
    {
        return _domainEvents.AsReadOnly();
    }

    public void ClearAllEvents()
    {
        _domainEvents.Clear();
    }
}