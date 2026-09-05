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
    public async Task Replaying_the_stream_keeps_the_status_time_the_write_recorded()
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
    public async Task Rebuilding_the_projection_keeps_the_status_time_and_the_error_ids()
    {
        await using var store = _fixture.NewStore();
        var chargePoint = SeedChargePoint(out var events);

        await AppendAsync(store, chargePoint.Id, events);

        var before = await LoadReadModelAsync(store, chargePoint.Id);

        using (var daemon = await store.BuildProjectionDaemonAsync())
        {
            await daemon.RebuildProjectionAsync<ChargePointProjection>(CancellationToken.None);
        }

        var after = await LoadReadModelAsync(store, chargePoint.Id);

        Assert.Equal(
            before.Connectors.Single().LastStatusTime,
            after.Connectors.Single().LastStatusTime);
        Assert.Equal(
            before.ConnectorErrors.Select(e => e.Id),
            after.ConnectorErrors.Select(e => e.Id));
    }

    [Fact]
    public async Task The_read_model_takes_the_connector_time_from_the_event()
    {
        await using var store = _fixture.NewStore();
        var chargePoint = SeedChargePoint(out var events, withError: false);
        var added = events.OfType<ConnectorAddedEvent>().Single();

        await AppendAsync(store, chargePoint.Id, events);

        var model = await LoadReadModelAsync(store, chargePoint.Id);

        Assert.Equal(added.AddedAt, model.Connectors.Single().LastStatusTime);
    }

    [Fact]
    public async Task The_read_model_takes_the_error_ids_from_the_events()
    {
        await using var store = _fixture.NewStore();
        var chargePoint = SeedChargePoint(out var events);
        var errorIds = events.OfType<ConnectorErrorLoggedEvent>().Select(e => e.ErrorId).ToList();

        await AppendAsync(store, chargePoint.Id, events);

        var model = await LoadReadModelAsync(store, chargePoint.Id);

        Assert.NotEmpty(errorIds);
        Assert.Equal(errorIds, model.ConnectorErrors.Select(e => e.Id));
    }

    /// <summary>Seed through the domain, never by writing documents directly.</summary>
    private static ChargePoint SeedChargePoint(out List<IDomainEvent> events, bool withError = true)
    {
        var chargePoint = new ChargePoint(Guid.NewGuid(), "CP-1", Guid.NewGuid(), 22.0, 0.0);
        chargePoint.AddConnector(1, "Connector 1");

        if (withError)
            chargePoint.LogConnectorError(1, "OverCurrentFailure", "phase 2");

        events = chargePoint.DomainEvents.ToList();
        return chargePoint;
    }

    private static async Task AppendAsync(IDocumentStore store, Guid streamId, IEnumerable<IDomainEvent> events)
    {
        await using var session = store.LightweightSession();
        session.Events.Append(streamId, events);
        await session.SaveChangesAsync();
    }

    private static async Task<ChargePointReadModel> LoadReadModelAsync(IDocumentStore store, Guid id)
    {
        await using var query = store.QuerySession();
        var model = await query.LoadAsync<ChargePointReadModel>(id);

        Assert.NotNull(model);
        return model!;
    }
}
