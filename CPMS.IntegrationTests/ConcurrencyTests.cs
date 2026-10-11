using CPMS.API.Entities;
using CPMS.API.Events.ChargeSession;
using CPMS.API.Infrastructure;
using CPMS.API.Repositories;
using JasperFx;
using Marten;
using Marten.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace CPMS.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class ConcurrencyTests
{
    private readonly PostgresFixture _fixture;

    public ConcurrencyTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task SaveAsync_after_another_request_changed_the_stream_throws_and_writes_nothing()
    {
        await using var services = _fixture.NewServices();
        var sessionId = await StartSessionAsync(services);

        await using var first = services.CreateAsyncScope();
        await using var second = services.CreateAsyncScope();
        var firstCopy = await Sessions(first).LoadAsync(sessionId);
        var secondCopy = await Sessions(second).LoadAsync(sessionId);

        firstCopy!.StopCharging("TAG-1", 5, "Local");
        await Sessions(first).SaveAsync(firstCopy);

        secondCopy!.StopCharging("TAG-1", 5, "Local");
        await Assert.ThrowsAnyAsync<ConcurrencyException>(() => Sessions(second).SaveAsync(secondCopy));

        Assert.Equal(1, await CountStopEventsAsync(services));
    }

    [Fact]
    public async Task AddAsync_with_an_id_already_in_use_throws()
    {
        await using var services = _fixture.NewServices();
        var id = Guid.NewGuid();

        await using (var first = services.CreateAsyncScope())
            await Tags(first).AddAsync(new ChargeTag(id, "TAG-1"));

        await using var second = services.CreateAsyncScope();
        await Assert.ThrowsAsync<ExistingStreamIdCollisionException>(
            () => Tags(second).AddAsync(new ChargeTag(id, "TAG-2")));
    }

    [Fact]
    public async Task SaveAsync_twice_on_the_same_loaded_aggregate_succeeds()
    {
        await using var services = _fixture.NewServices();
        var id = Guid.NewGuid();
        await using (var create = services.CreateAsyncScope())
            await Tags(create).AddAsync(new ChargeTag(id, "TAG-1"));

        await using var scope = services.CreateAsyncScope();
        var tag = await Tags(scope).LoadAsync(id);
        tag!.Block();
        await Tags(scope).SaveAsync(tag);
        tag.Unblock();
        await Tags(scope).SaveAsync(tag);

        await using var check = services.CreateAsyncScope();
        Assert.False((await Tags(check).LoadAsync(id))!.Blocked);
    }

    [Fact]
    public async Task A_concurrency_conflict_is_answered_with_409()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var handler = new ApiExceptionHandler(NullLogger<ApiExceptionHandler>.Instance);

        await handler.TryHandleAsync(context, new ConcurrencyException("conflict", new Exception()), CancellationToken.None);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
    }

    private static async Task<Guid> StartSessionAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var session = new ChargeSession(Guid.NewGuid(), 1_000_000, Guid.NewGuid(), 1, "TAG-1", 0);
        await Sessions(scope).AddAsync(session);
        return session.Id;
    }

    private static IAggregateRepository<ChargeSession> Sessions(AsyncServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IAggregateRepository<ChargeSession>>();

    private static IAggregateRepository<ChargeTag> Tags(AsyncServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<IAggregateRepository<ChargeTag>>();

    private static async Task<int> CountStopEventsAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IQuerySession>()
            .Events.QueryRawEventDataOnly<ChargeSessionStoppedEvent>().CountAsync();
    }
}
