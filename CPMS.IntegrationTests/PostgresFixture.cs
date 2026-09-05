using CPMS.API.Projections;
using JasperFx.Events.Projections;
using Marten;
using Testcontainers.PostgreSql;

namespace CPMS.IntegrationTests;

/// <summary>
/// One PostgreSQL container for the whole test run. Each store gets its own schema so tests
/// stay independent without a clean-up dance.
/// </summary>
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .Build();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    /// <summary>Marten configured the way <c>Program.cs</c> configures it, in an isolated schema.</summary>
    public DocumentStore NewStore() => DocumentStore.For(options =>
    {
        options.Connection(_container.GetConnectionString());
        options.DatabaseSchemaName = "test_" + Guid.NewGuid().ToString("n")[..8];
        options.UseNewtonsoftForSerialization();

        options.Projections.Add<ChargePointProjection>(ProjectionLifecycle.Inline);
    });
}

[CollectionDefinition(nameof(PostgresCollection))]
public class PostgresCollection : ICollectionFixture<PostgresFixture>;
