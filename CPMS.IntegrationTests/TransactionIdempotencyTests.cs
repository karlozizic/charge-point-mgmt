using CPMS.API.Entities;
using CPMS.API.Events.ChargeSession;
using CPMS.API.Handlers.ChargeSession;
using CPMS.API.Projections;
using CPMS.BuildingBlocks.Domain;
using CPMS.Core.Models.OCPP_1._6;
using CPMS.Core.Models.Responses;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CPMS.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class TransactionIdempotencyTests
{
    private const string Tag = "TAG-1";

    private readonly PostgresFixture _fixture;

    public TransactionIdempotencyTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task StartTransaction_in_parallel_on_many_chargers_issues_distinct_ids()
    {
        await using var services = _fixture.NewServices();
        var chargers = Enumerable.Range(1, 20).Select(i => $"CP-{i}").ToArray();
        await SeedAsync(services, chargers);

        var responses = await Task.WhenAll(chargers.Select(cp => StartAsync(services, cp, meterStart: 0)));

        Assert.All(responses, r => Assert.Equal(AuthorizationStatus.Accepted, r.IdTagInfo.Status));
        Assert.Equal(chargers.Length, responses.Select(r => r.TransactionId).Distinct().Count());
    }

    [Fact]
    public async Task StartTransaction_issues_ids_in_order_above_the_old_random_range()
    {
        await using var services = _fixture.NewServices();
        await SeedAsync(services, "CP-1", "CP-2");

        var first = await StartAsync(services, "CP-1", meterStart: 0);
        var second = await StartAsync(services, "CP-2", meterStart: 0);

        Assert.True(first.TransactionId >= 1_000_000);
        Assert.Equal(first.TransactionId + 1, second.TransactionId);
    }

    [Fact]
    public async Task StartTransaction_retried_returns_the_same_id_and_opens_one_session()
    {
        await using var services = _fixture.NewServices();
        await SeedAsync(services, "CP-1");

        var first = await StartAsync(services, "CP-1", meterStart: 12.5);
        var retry = await StartAsync(services, "CP-1", meterStart: 12.5);

        Assert.Equal(first.TransactionId, retry.TransactionId);
        Assert.Equal(AuthorizationStatus.Accepted, retry.IdTagInfo.Status);
        Assert.Equal(1, await CountSessionsAsync(services));
    }

    [Fact]
    public async Task StartTransaction_with_the_same_values_after_a_stop_opens_a_new_session()
    {
        await using var services = _fixture.NewServices();
        await SeedAsync(services, "CP-1");

        var first = await StartAsync(services, "CP-1", meterStart: 0);
        await StopAsync(services, first.TransactionId!.Value, meterStop: 5);
        var second = await StartAsync(services, "CP-1", meterStart: 0);

        Assert.NotEqual(first.TransactionId, second.TransactionId);
        Assert.Equal(2, await CountSessionsAsync(services));
    }

    [Fact]
    public async Task StopTransaction_retried_is_accepted_and_stops_the_session_once()
    {
        await using var services = _fixture.NewServices();
        await SeedAsync(services, "CP-1");
        var started = await StartAsync(services, "CP-1", meterStart: 0);

        var first = await StopAsync(services, started.TransactionId!.Value, meterStop: 5);
        var retry = await StopAsync(services, started.TransactionId!.Value, meterStop: 5);

        Assert.Equal(AuthorizationStatus.Accepted, first.IdTagInfo.Status);
        Assert.Equal(AuthorizationStatus.Accepted, retry.IdTagInfo.Status);
        Assert.Equal(1, await CountStopEventsAsync(services));
    }

    private static async Task SeedAsync(IServiceProvider services, params string[] chargers)
    {
        await using var scope = services.CreateAsyncScope();
        var session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();

        Append(session, new ChargeTag(Guid.NewGuid(), Tag));
        foreach (var ocppId in chargers)
        {
            var chargePoint = new ChargePoint(Guid.NewGuid(), ocppId, Guid.NewGuid(), 22, 0);
            chargePoint.AddConnector(1, "Type 2");
            Append(session, chargePoint);
        }

        await session.SaveChangesAsync();
    }

    private static void Append<T>(IDocumentSession session, T aggregate) where T : Entity, IAggregateRoot
    {
        session.Events.Append(aggregate.Id, aggregate.DomainEvents.Cast<object>().ToArray());
    }

    private static async Task<StartTransactionResponse> StartAsync(IServiceProvider services, string ocppId, double meterStart)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IMediator>().Send(new StartTransactionCommand
        {
            OcppChargerId = ocppId,
            ConnectorId = 1,
            TagId = Tag,
            MeterStart = meterStart
        });
    }

    private static async Task<StopTransactionResponse> StopAsync(IServiceProvider services, int transactionId, double meterStop)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IMediator>().Send(
            new StopTransactionCommand(transactionId, DateTime.UtcNow, meterStop, Tag, "Local"));
    }

    private static async Task<int> CountSessionsAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IQuerySession>()
            .Query<ChargeSessionReadModel>().CountAsync();
    }

    private static async Task<int> CountStopEventsAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IQuerySession>()
            .Events.QueryRawEventDataOnly<ChargeSessionStoppedEvent>().CountAsync();
    }
}
