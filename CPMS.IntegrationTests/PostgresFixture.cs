using CPMS.API.Infrastructure;
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

    /// <summary>
    /// A store configured by the same code path as the application, in an isolated schema.
    /// Registering the projections here instead would let the test store drift from Program.cs.
    /// </summary>
    public DocumentStore NewStore() => DocumentStore.For(options =>
    {
        MartenConfiguration.Configure(options, _container.GetConnectionString());
        options.DatabaseSchemaName = "test_" + Guid.NewGuid().ToString("n")[..8];
    });
}

[CollectionDefinition(nameof(PostgresCollection))]
public class PostgresCollection : ICollectionFixture<PostgresFixture>;
