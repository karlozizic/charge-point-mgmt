using Marten;
using Weasel.Core;
using Weasel.Core.Migrations;
using Weasel.Postgresql;

namespace CPMS.API.Infrastructure;

public class TransactionIdSequence : FeatureSchemaBase
{
    public const string Name = "mt_transaction_id";
    private const long StartFrom = 1_000_000;

    private readonly StoreOptions _options;

    public TransactionIdSequence(StoreOptions options)
        : base(nameof(TransactionIdSequence), options.Advanced.Migrator)
    {
        _options = options;
    }

    protected override IEnumerable<ISchemaObject> schemaObjects()
    {
        yield return new Sequence(new PostgresqlObjectName(_options.DatabaseSchemaName, Name), StartFrom);
    }
}

public static class TransactionIdSequenceExtensions
{
    public static async Task<int> NextTransactionIdAsync(this IQuerySession session, CancellationToken cancellationToken)
    {
        await session.Database.EnsureStorageExistsAsync(typeof(TransactionIdSequence), cancellationToken);

        var name = $"{session.DocumentStore.Options.DatabaseSchemaName}.{TransactionIdSequence.Name}";
        var values = await session.QueryAsync<int>("select nextval(?)::int", cancellationToken, name);
        return values[0];
    }
}
