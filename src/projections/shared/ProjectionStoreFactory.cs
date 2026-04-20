using Npgsql;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Projections.Shared;

/// <summary>
/// Centralizes creation of PostgresProjectionStore instances.
/// Bootstrap modules call Create&lt;T&gt; instead of constructing stores directly.
///
/// R2.A.D.3c / R-POSTGRES-POOL-BREAKER-01: optionally carries the shared
/// <c>"postgres-pool"</c> breaker so every store built from this factory
/// routes its connection acquisitions through the breaker.
/// </summary>
public sealed class ProjectionStoreFactory
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ICircuitBreaker? _poolBreaker;

    public ProjectionStoreFactory(NpgsqlDataSource dataSource, ICircuitBreaker? poolBreaker = null)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        _dataSource = dataSource;
        _poolBreaker = poolBreaker;
    }

    public PostgresProjectionStore<TState> Create<TState>(
        string schema,
        string table,
        string aggregateType) where TState : class =>
        new(_dataSource, schema, table, aggregateType, _poolBreaker);
}
