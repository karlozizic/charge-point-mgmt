using CPMS.API.Entities;
using CPMS.API.Projections;
using CPMS.BuildingBlocks.Domain;
using Marten;

namespace CPMS.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class MeterValueProjectionTests
{
    private readonly PostgresFixture _fixture;

    public MeterValueProjectionTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Each_recorded_meter_value_becomes_its_own_document()
    {
        await using var store = _fixture.NewStore();
        var session = SeedSession(out var events, readings: 3);

        await AppendAsync(store, session.Id, events);

        var readings = await ReadingsAsync(store, session.Id);

        Assert.Equal(3, readings.Count);
        Assert.Equal([1.0, 2.0, 3.0], readings.Select(r => r.EnergyConsumed));
        Assert.All(readings, r => Assert.Equal(session.Id, r.SessionId));
    }

    [Fact]
    public async Task The_session_document_carries_the_latest_energy_without_the_readings()
    {
        await using var store = _fixture.NewStore();
        var session = SeedSession(out var events, readings: 3);

        await AppendAsync(store, session.Id, events);

        await using var query = store.QuerySession();
        var model = await query.LoadAsync<ChargeSessionReadModel>(session.Id);

        Assert.NotNull(model);
        Assert.Equal(3.0, model!.EnergyDeliveredKWh);
    }

    [Fact]
    public async Task A_stop_without_a_closing_reading_keeps_the_energy_from_the_last_reading()
    {
        await using var store = _fixture.NewStore();
        var session = SeedSession(out var events, readings: 3);

        session.StopCharging("TAG-1", 0, "Local");
        events.AddRange(session.DomainEvents.Skip(events.Count));

        await AppendAsync(store, session.Id, events);

        await using var query = store.QuerySession();
        var model = await query.LoadAsync<ChargeSessionReadModel>(session.Id);

        Assert.Equal(3.0, model!.EnergyDeliveredKWh);
        Assert.Equal(nameof(SessionStatus.Stopped), model.Status);
    }

    [Fact]
    public async Task Rebuilding_produces_the_same_documents_rather_than_a_second_copy()
    {
        await using var store = _fixture.NewStore();
        var session = SeedSession(out var events, readings: 4);
        await AppendAsync(store, session.Id, events);

        var before = await ReadingsAsync(store, session.Id);

        using (var daemon = await store.BuildProjectionDaemonAsync())
        {
            await daemon.RebuildProjectionAsync<MeterValueProjection>(CancellationToken.None);
        }

        var after = await ReadingsAsync(store, session.Id);

        Assert.Equal(4, after.Count);
        Assert.Equal(before.Select(r => r.Id), after.Select(r => r.Id));
    }

    private static ChargeSession SeedSession(out List<IDomainEvent> events, int readings)
    {
        var session = new ChargeSession(Guid.NewGuid(), 4711, Guid.NewGuid(), 1, "TAG-1", 0);

        for (var i = 1; i <= readings; i++)
        {
            session.AddMeterValue(11.0, i, i * 10.0, new DateTime(2026, 1, 1, 0, i, 0, DateTimeKind.Utc));
        }

        events = session.DomainEvents.ToList();
        return session;
    }

    private static async Task AppendAsync(IDocumentStore store, Guid streamId, IEnumerable<IDomainEvent> events)
    {
        await using var session = store.LightweightSession();
        session.Events.Append(streamId, events);
        await session.SaveChangesAsync();
    }

    private static async Task<IReadOnlyList<MeterValueReadModel>> ReadingsAsync(IDocumentStore store, Guid sessionId)
    {
        await using var query = store.QuerySession();
        return await query.Query<MeterValueReadModel>()
            .Where(r => r.SessionId == sessionId)
            .OrderBy(r => r.Timestamp)
            .ToListAsync();
    }
}
