using CPMS.BuildingBlocks.Domain;
using Marten;
using MediatR;

namespace CPMS.API.Repositories;

/// <summary>
/// Loads an aggregate by replaying its event stream and persists the events it raised.
/// Every aggregate is a Marten event stream keyed by its Id; there is no table of aggregate state.
/// </summary>
public interface IAggregateRepository<T> where T : Entity, IAggregateRoot
{
    Task<T?> LoadAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends the aggregate's pending domain events to its stream, commits (inline projections run in
    /// the same transaction) and then publishes each event as a MediatR notification.
    /// </summary>
    Task SaveAsync(T aggregate, CancellationToken cancellationToken = default);
}

public class MartenAggregateRepository<T> : IAggregateRepository<T> where T : Entity, IAggregateRoot
{
    private readonly IDocumentSession _session;
    private readonly IMediator _mediator;

    public MartenAggregateRepository(IDocumentSession session, IMediator mediator)
    {
        _session = session;
        _mediator = mediator;
    }

    public Task<T?> LoadAsync(Guid id, CancellationToken cancellationToken = default) =>
        _session.Events.AggregateStreamAsync<T>(id, token: cancellationToken);

    public async Task SaveAsync(T aggregate, CancellationToken cancellationToken = default)
    {
        var events = aggregate.DomainEvents.ToList();
        if (events.Count == 0)
            return;

        _session.Events.Append(aggregate.Id, events);
        await _session.SaveChangesAsync(cancellationToken);
        aggregate.ClearDomainEvents();

        // Published after commit and in-process: a crash between the two lines loses the side effect.
        // That is the known "no outbox" gap.
        foreach (var domainEvent in events)
        {
            await _mediator.Publish(domainEvent, cancellationToken);
        }
    }
}
