using CPMS.API.Entities;
using CPMS.API.Events.ChargePoint;
using CPMS.API.Events.Connector;
using CPMS.API.Projections;
using CPMS.BuildingBlocks.Domain;
using Marten;

namespace CPMS.IntegrationTests;

/// <summary>
/// Replay and rebuild must produce exactly what the original write produced. Anything an
/// <c>Apply</c> method or a projection invents at replay time shows up here.
/// </summary>
[Collection(nameof(PostgresCollection))]
public class ChargePointReplayTests
{
    private readonly PostgresFixture _fixture;

    public ChargePointReplayTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task AggregateStream_after_a_connector_is_added_keeps_the_recorded_status_time()
    {
        await using var store = _fixture.NewStore();
        var chargePoint = SeedChargePoint(out var events);
        var addedAt = events.OfType<ConnectorAddedEvent>().Single().AddedAt;

        await AppendAsync(store, chargePoint.Id, events);

        await using var query = store.QuerySession();
        var replayed = await query.Events.AggregateStreamAsync<ChargePoint>(chargePoint.Id);

        Assert.NotNull(replayed);
        Assert.Equal(addedAt, replayed!.Connectors.Single().LastStatusTime);
    }

    [Fact]
    public async Task Projection_after_a_connector_is_added_takes_the_status_time_from_the_event()
    {
        await using var store = _fixture.NewStore();
        var chargePoint = SeedChargePoint(out var events);
        var addedAt = events.OfType<ConnectorAddedEvent>().Single().AddedAt;

        await AppendAsync(store, chargePoint.Id, events);

        var model = await LoadReadModelAsync(store, chargePoint.Id);

        Assert.Equal(addedAt, model.Connectors.Single().LastStatusTime);
    }

    [Fact]
    public async Task Rebuild_after_a_connector_is_added_keeps_the_recorded_status_time()
    {
        await using var store = _fixture.NewStore();
        var chargePoint = SeedChargePoint(out var events);
        var addedAt = events.OfType<ConnectorAddedEvent>().Single().AddedAt;

        await AppendAsync(store, chargePoint.Id, events);
        await RebuildAsync(store);

        var model = await LoadReadModelAsync(store, chargePoint.Id);

        // Assert against the event, not against the document written before the rebuild: a
        // before-and-after comparison passes even when both sides invent the same wrong value.
        Assert.Equal(addedAt, model.Connectors.Single().LastStatusTime);
    }

    [Fact]
    public async Task Rebuild_after_connector_errors_keeps_the_recorded_error_ids()
    {
        await using var store = _fixture.NewStore();
        var chargePoint = SeedChargePoint(out var events, errorCount: 2);
        var errorIds = events.OfType<ConnectorErrorLoggedEvent>().Select(e => e.ErrorId).ToList();

        await AppendAsync(store, chargePoint.Id, events);
        await RebuildAsync(store);

        var model = await LoadReadModelAsync(store, chargePoint.Id);

        Assert.Equal(2, errorIds.Count);
        Assert.Equal(errorIds, model.ConnectorErrors.Select(e => e.Id));
    }

    [Fact]
    public async Task Projection_of_an_event_written_before_AddedAt_existed_leaves_the_time_unknown()
    {
        await using var store = _fixture.NewStore();
        var chargePointId = Guid.NewGuid();

        // Marten deserializes an old payload into the current class, so the field it never
        // carried arrives as default. That must read as "unknown", not as year 1.
        var legacyAdded = new ConnectorAddedEvent(chargePointId, 1, "Connector 1", default);

        await AppendAsync(store, chargePointId, [
            new ChargePointCreatedEvent(chargePointId, "CP-LEGACY", Guid.NewGuid(), 22.0, 0.0),
            legacyAdded
        ]);

        var model = await LoadReadModelAsync(store, chargePointId);

        Assert.Null(model.Connectors.Single().LastStatusTime);
    }

    /// <summary>Seed through the domain, never by writing documents directly.</summary>
    private static ChargePoint SeedChargePoint(out List<IDomainEvent> events, int errorCount = 0)
    {
        var chargePoint = new ChargePoint(Guid.NewGuid(), "CP-1", Guid.NewGuid(), 22.0, 0.0);
        chargePoint.AddConnector(1, "Connector 1");

        for (var i = 0; i < errorCount; i++)
            chargePoint.LogConnectorError(1, "OverCurrentFailure", $"phase {i + 1}");

        events = chargePoint.DomainEvents.ToList();
        return chargePoint;
    }

    private static async Task AppendAsync(IDocumentStore store, Guid streamId, IEnumerable<IDomainEvent> events)
    {
        await using var session = store.LightweightSession();
        session.Events.Append(streamId, events);
        await session.SaveChangesAsync();
    }

    private static async Task RebuildAsync(IDocumentStore store)
    {
        using var daemon = await store.BuildProjectionDaemonAsync();
        await daemon.RebuildProjectionAsync<ChargePointProjection>(CancellationToken.None);
    }

    private static async Task<ChargePointReadModel> LoadReadModelAsync(IDocumentStore store, Guid id)
    {
        await using var query = store.QuerySession();
        var model = await query.LoadAsync<ChargePointReadModel>(id);

        Assert.NotNull(model);
        return model!;
    }
}
