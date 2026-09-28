using CPMS.API.Entities;
using CPMS.API.Events.ChargeSession;
using CPMS.API.Handlers.ChargeSession;
using CPMS.BuildingBlocks.Domain;
using Marten;

namespace CPMS.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class ChargeSessionStatsTests
{
    private readonly PostgresFixture _fixture;

    public ChargeSessionStatsTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task An_empty_store_reports_zeros_rather_than_failing()
    {
        await using var store = _fixture.NewStore();

        var stats = await StatsAsync(store, new GetChargeSessionStatsQuery());

        Assert.Equal(0, stats.TotalSessions);
        Assert.Equal(0, stats.TotalEnergyDelivered);
        Assert.Equal(0, stats.AverageSessionDuration);
        Assert.Equal(0, stats.AverageEnergyPerSession);
    }

    [Fact]
    public async Task Started_and_stopped_sessions_are_counted_apart()
    {
        await using var store = _fixture.NewStore();
        await AppendAsync(store, Running(energy: 1));
        await AppendAsync(store, Running(energy: 2));
        await AppendAsync(store, Finished(energy: 3, stopMeter: 3));

        var stats = await StatsAsync(store, new GetChargeSessionStatsQuery());

        Assert.Equal(3, stats.TotalSessions);
        Assert.Equal(2, stats.ActiveSessions);
        Assert.Equal(1, stats.CompletedSessions);
    }

    [Fact]
    public async Task Energy_is_summed_and_averaged_over_the_sessions_that_delivered_any()
    {
        await using var store = _fixture.NewStore();
        await AppendAsync(store, Finished(energy: 2, stopMeter: 2));
        await AppendAsync(store, Finished(energy: 4, stopMeter: 4));
        await AppendAsync(store, Running(energy: 0));

        var stats = await StatsAsync(store, new GetChargeSessionStatsQuery());

        Assert.Equal(3, stats.TotalSessions);
        Assert.Equal(6, stats.TotalEnergyDelivered, 3);
        Assert.Equal(3, stats.AverageEnergyPerSession, 3);
    }

    [Fact]
    public async Task Average_duration_covers_only_the_sessions_that_stopped()
    {
        await using var store = _fixture.NewStore();
        var chargePointId = Guid.NewGuid();
        var start = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);

        await AppendAsync(store, TimedSession(chargePointId, start, start.AddMinutes(10)));
        await AppendAsync(store, TimedSession(chargePointId, start, start.AddMinutes(20)));
        await AppendAsync(store, Running(energy: 1));

        var stats = await StatsAsync(store, new GetChargeSessionStatsQuery());

        Assert.Equal(3, stats.TotalSessions);
        Assert.Equal(15, stats.AverageSessionDuration, 3);
    }

    [Fact]
    public async Task A_charge_point_filter_narrows_every_number()
    {
        await using var store = _fixture.NewStore();
        var wanted = Guid.NewGuid();
        await AppendAsync(store, Finished(energy: 5, stopMeter: 5, chargePointId: wanted));
        await AppendAsync(store, Finished(energy: 9, stopMeter: 9));

        var stats = await StatsAsync(store, new GetChargeSessionStatsQuery { ChargePointId = wanted.ToString() });

        Assert.Equal(1, stats.TotalSessions);
        Assert.Equal(5, stats.TotalEnergyDelivered, 3);
    }

    [Fact]
    public async Task A_date_range_excludes_sessions_that_started_outside_it()
    {
        await using var store = _fixture.NewStore();
        var chargePointId = Guid.NewGuid();
        var inside = new DateTime(2026, 6, 15, 8, 0, 0, DateTimeKind.Utc);
        var outside = new DateTime(2026, 1, 5, 8, 0, 0, DateTimeKind.Utc);

        await AppendAsync(store, TimedSession(chargePointId, inside, inside.AddMinutes(30)));
        await AppendAsync(store, TimedSession(chargePointId, outside, outside.AddMinutes(30)));

        var stats = await StatsAsync(store, new GetChargeSessionStatsQuery
        {
            FromDate = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            ToDate = new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc)
        });

        Assert.Equal(1, stats.TotalSessions);
    }

    private static async Task<CPMS.API.Dtos.ChargeSessionStatsDto> StatsAsync(
        IDocumentStore store, GetChargeSessionStatsQuery query)
    {
        await using var querySession = store.QuerySession();
        return await new GetChargeSessionStatsQueryHandler(querySession).Handle(query, CancellationToken.None);
    }

    private static (Guid Id, List<IDomainEvent> Events) Running(double energy, Guid? chargePointId = null)
    {
        var session = new ChargeSession(Guid.NewGuid(), 1, chargePointId ?? Guid.NewGuid(), 1, "TAG-1", 0);
        if (energy > 0)
        {
            session.AddMeterValue(11.0, energy, 50.0, new DateTime(2026, 1, 1, 0, 1, 0, DateTimeKind.Utc));
        }

        return (session.Id, session.DomainEvents.ToList());
    }

    private static (Guid Id, List<IDomainEvent> Events) Finished(
        double energy, double stopMeter, Guid? chargePointId = null)
    {
        var session = new ChargeSession(Guid.NewGuid(), 1, chargePointId ?? Guid.NewGuid(), 1, "TAG-1", 0);
        session.AddMeterValue(11.0, energy, 50.0, new DateTime(2026, 1, 1, 0, 1, 0, DateTimeKind.Utc));
        session.StopCharging("TAG-1", stopMeter, "Local");

        return (session.Id, session.DomainEvents.ToList());
    }

    private static (Guid Id, List<IDomainEvent> Events) TimedSession(Guid chargePointId, DateTime start, DateTime stop)
    {
        var id = Guid.NewGuid();
        return (id, [
            new ChargeSessionStartedEvent(id, 1, chargePointId, 1, "TAG-1", start, 0),
            new ChargeSessionStoppedEvent(id, chargePointId, 1, "TAG-1", stop, 1, "Local")
        ]);
    }

    private static async Task AppendAsync(IDocumentStore store, (Guid Id, List<IDomainEvent> Events) seeded)
    {
        await using var session = store.LightweightSession();
        session.Events.Append(seeded.Id, seeded.Events);
        await session.SaveChangesAsync();
    }
}
