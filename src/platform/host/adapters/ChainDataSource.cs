using Npgsql;

namespace Whyce.Platform.Host.Adapters;

/// <summary>
/// phase1.5-S5.2.1 / PC-4 (POSTGRES-POOL-01): named singleton wrapper
/// around the <see cref="NpgsqlDataSource"/> for the <c>chain</c>
/// logical pool. Backs <see cref="WhyceChainPostgresAdapter"/> and is
/// kept separate from the <see cref="EventStoreDataSource"/> so a
/// chain-store outage cannot exhaust the event-store pool (and vice
/// versa). The two pools may point at the same Postgres connection
/// string in development; the architecture is the source of truth that
/// they are logically distinct.
/// </summary>
public sealed class ChainDataSource
{
    public const string PoolName = "chain";

    public NpgsqlDataSource Inner { get; }

    public ChainDataSource(NpgsqlDataSource inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        Inner = inner;
    }
}
