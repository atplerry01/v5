using Npgsql;

namespace Whycespace.Projections.Shared;

/// <summary>
/// Centralizes creation of PostgresProjectionStore instances.
/// Bootstrap modules call Create&lt;T&gt; instead of constructing stores directly.
/// </summary>
public sealed class ProjectionStoreFactory
{
    private readonly NpgsqlDataSource _dataSource;

    public ProjectionStoreFactory(NpgsqlDataSource dataSource)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        _dataSource = dataSource;
    }

    public PostgresProjectionStore<TState> Create<TState>(
        string schema,
        string table,
        string aggregateType) where TState : class =>
        new(_dataSource, schema, table, aggregateType);
}
