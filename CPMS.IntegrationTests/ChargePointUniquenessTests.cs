using CPMS.API.Handlers.ChargePoint;
using CPMS.API.Projections;
using CPMS.BuildingBlocks.Domain;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CPMS.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class ChargePointUniquenessTests
{
    private readonly PostgresFixture _fixture;

    public ChargePointUniquenessTests(PostgresFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Creating_a_charge_point_with_an_id_already_registered_is_a_business_rule_violation()
    {
        await using var services = _fixture.NewServices();
        await CreateAsync(services, "CP-1");

        await Assert.ThrowsAsync<BusinessRuleValidationException>(() => CreateAsync(services, "CP-1"));

        Assert.Equal(1, await CountAsync(services, "CP-1"));
    }

    [Fact]
    public async Task Parallel_creates_with_the_same_id_register_exactly_one_charge_point()
    {
        await using var services = _fixture.NewServices();

        var results = await Task.WhenAll(Enumerable.Range(0, 10).Select(async _ =>
        {
            try
            {
                await CreateAsync(services, "CP-RACE");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }));

        Assert.Equal(1, results.Count(created => created));
        Assert.Equal(1, await CountAsync(services, "CP-RACE"));
    }

    private static async Task CreateAsync(IServiceProvider services, string ocppChargerId)
    {
        await using var scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IMediator>().Send(new CreateChargePointCommand
        {
            OcppChargerId = ocppChargerId,
            LocationId = Guid.NewGuid(),
            MaxPower = 22
        });
    }

    private static async Task<int> CountAsync(IServiceProvider services, string ocppChargerId)
    {
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IQuerySession>()
            .Query<ChargePointReadModel>().CountAsync(cp => cp.OcppChargerId == ocppChargerId);
    }
}
