using Npgsql;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// phase1.5-S5.2.1 / PC-4 (POSTGRES-POOL-01): named singleton wrapper
/// around the <see cref="NpgsqlDataSource"/> for the <c>chain</c>
/// logical pool. Backs <see cref="WhyceChainPostgresAdapter"/> and is
/// kept separate from the <see cref="EventStoreDataSource"/> so a
/// chain-store outage cannot exhaust the event-store pool (and vice
/// versa). The two pools may point at the same Postgres connection
/// string in development; the architecture is the source of truth that
/// they are logically distinct.
///
/// R2.A.D.3c / R-POSTGRES-POOL-BREAKER-01: shares the same canonical
/// <see cref="OpenAsync"/> acquire shape as <see cref="EventStoreDataSource"/>
/// and <see cref="ProjectionsDataSource"/>. Routes pooled acquisitions
/// through the shared <c>"postgres-pool"</c> <see cref="ICircuitBreaker"/>
/// when configured.
/// </summary>
public sealed class ChainDataSource
{
    public const string PoolName = "chain";

    public NpgsqlDataSource Inner { get; }

    private readonly ICircuitBreaker? _poolBreaker;

    public ChainDataSource(NpgsqlDataSource inner, ICircuitBreaker? poolBreaker = null)
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
