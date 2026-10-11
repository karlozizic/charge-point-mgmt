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
    Task<T?> FindAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(T aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends the aggregate's pending domain events to its stream, commits (inline projections run in
    /// the same transaction) and then publishes each event as a MediatR notification.
    /// </summary>
    Task UpdateAsync(T aggregate, CancellationToken cancellationToken = default);
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

    public async Task<T?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var stream = await _session.Events.FetchForWriting<T>(id, cancellationToken);
        return stream.Aggregate;
    }

    public async Task AddAsync(T aggregate, CancellationToken cancellationToken = default)
    {
        var events = aggregate.DomainEvents.ToList();
        if (events.Count == 0)
            return;

        _session.Events.StartStream<T>(aggregate.Id, events);
        await CommitAsync(aggregate, events, cancellationToken);
    }

    public async Task UpdateAsync(T aggregate, CancellationToken cancellationToken = default)
    {
        var events = aggregate.DomainEvents.ToList();
        if (events.Count == 0)
            return;

        _session.Events.Append(aggregate.Id, events);
        await CommitAsync(aggregate, events, cancellationToken);
    }

    private async Task CommitAsync(T aggregate, List<IDomainEvent> events, CancellationToken cancellationToken)
    {
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
