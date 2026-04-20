using Npgsql;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// phase1.5-S5.2.1 / PC-4 (POSTGRES-POOL-01): named singleton wrapper
/// around the <see cref="NpgsqlDataSource"/> for the <c>event-store</c>
/// logical pool. Backs every adapter that touches the canonical
/// Postgres connection string: event store, idempotency store, sequence
/// store, outbox enqueue, outbox publisher, outbox depth sampler.
///
/// The wrapper class exists so DI can disambiguate this pool from the
/// <see cref="ChainDataSource"/> (Microsoft.Extensions.DependencyInjection
/// has no native keyed-singleton for arbitrary types in this codebase's
/// usage pattern, and the wider repo prefers explicit wrapper types
/// over <c>IServiceProviderIsKeyedService</c>).
///
/// R2.A.D.3c / R-POSTGRES-POOL-BREAKER-01: the wrapper now exposes a
/// canonical <see cref="OpenAsync"/> acquire method that routes through
/// the shared <c>"postgres-pool"</c> <see cref="ICircuitBreaker"/> when
/// one is supplied at construction. Adapters MUST use
/// <see cref="OpenAsync"/> instead of
/// <c>Inner.OpenInstrumentedAsync(PoolName, ct)</c> so the breaker gate
/// is applied uniformly.
/// </summary>
public sealed class EventStoreDataSource
{
    public const string PoolName = "event-store";

    public NpgsqlDataSource Inner { get; }

    private readonly ICircuitBreaker? _poolBreaker;

    public EventStoreDataSource(NpgsqlDataSource inner, ICircuitBreaker? poolBreaker = null)
    {
        ArgumentNullException.ThrowIfNull(inner);
        Inner = inner;
        _poolBreaker = poolBreaker;
    }

    /// <summary>
    /// R2.A.D.3c: acquires a pooled connection through the shared
    /// <c>"postgres-pool"</c> breaker (when configured) and the canonical
    /// <see cref="PostgresPoolMetrics.OpenInstrumentedAsync"/> seam.
    /// Throws <see cref="CircuitBreakerOpenException"/> when the shared
    /// breaker is Open — callers handle per the R2.A.D.3c posture table.
    /// </summary>
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
