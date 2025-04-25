namespace CPMS.BuildingBlocks.Infrastructure.Eventbus;

public interface IEventsBus : IDisposable
{
    Task Publish<T>(T @event)
        where T : IntegrationEvent;

    void Subscribe<T>(IIntegrationEventHandler<T> handler)
        where T : IntegrationEvent;

    void StartConsuming();
}