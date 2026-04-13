using Npgsql;

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
/// </summary>
public sealed class EventStoreDataSource
{
    public const string PoolName = "event-store";

    public NpgsqlDataSource Inner { get; }

    public EventStoreDataSource(NpgsqlDataSource inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        Inner = inner;
    }
}
