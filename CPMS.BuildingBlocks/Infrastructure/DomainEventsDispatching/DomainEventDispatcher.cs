using MediatR;

namespace CPMS.BuildingBlocks.Infrastructure.DomainEventsDispatching;

public class DomainEventsDispatcher : IDomainEventsDispatcher
{
    private readonly IMediator _mediator;
    private readonly MartenDomainEventsAccessor _domainEventsAccessor;

    public DomainEventsDispatcher(
        IMediator mediator,
        MartenDomainEventsAccessor domainEventsAccessor)
    {
        _mediator = mediator;
        _domainEventsAccessor = domainEventsAccessor;
    }

    public async Task DispatchEventsAsync()
    {
        var domainEvents = _domainEventsAccessor.GetAllEvents();

        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent);
        }
        
        _domainEventsAccessor.ClearAllEvents();
    }
}