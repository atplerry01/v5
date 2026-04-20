using Npgsql;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// phase1.5-S5.2.2 / KC-4 (PROJECTIONS-POOL-01): named singleton
/// wrapper around the <see cref="NpgsqlDataSource"/> for the
/// <c>projections</c> logical pool. Backs adapters that touch the
/// projections-side database: <see cref="PostgresProjectionWriter"/>,
/// the <c>TodoController</c> direct read-side query, and the
/// <c>TodoProjectionHandler</c> in the projections assembly (which
/// receives the inner <see cref="NpgsqlDataSource"/> directly because
/// the <c>Whycespace.Projections</c> assembly cannot reference the
/// host-adapters layer).
///
/// Mirrors <see cref="EventStoreDataSource"/> and
/// <see cref="ChainDataSource"/> shape exactly. The wrapper class
/// exists so DI can disambiguate this pool from the other two
/// declared pools without keyed-service plumbing. Pool sizing flows
/// from <c>Postgres.Pools.Projections.*</c> configuration via
/// <c>BuildDataSource</c>.
///
/// R2.A.D.3c / R-POSTGRES-POOL-BREAKER-01: shares the canonical
/// <see cref="OpenAsync"/> acquire shape with the other two wrappers.
/// </summary>
public sealed class ProjectionsDataSource
{
    public const string PoolName = "projections";

    public NpgsqlDataSource Inner { get; }

    private readonly ICircuitBreaker? _poolBreaker;

    public ProjectionsDataSource(NpgsqlDataSource inner, ICircuitBreaker? poolBreaker = null)
    {
        ArgumentNullException.ThrowIfNull(inner);
        Inner = inner;
        _poolBreaker = poolBreaker;
    }

    public Task<NpgsqlConnection> OpenAsync(CancellationToken cancellationToken = default)
    {
        if (_poolBreaker is null)
        {
            return Inner.OpenInstrumentedAsync(PoolName, cancellationToken);
        }
        return _poolBreaker.ExecuteAsync(
            ct => Inner.OpenInstrumentedAsync(PoolName, ct),
            cancellationToken);
    }
}
