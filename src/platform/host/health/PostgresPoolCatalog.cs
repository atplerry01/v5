namespace Whycespace.Platform.Host.Health;

/// <summary>
/// phase1.5-S5.2.4 / HC-6 (POSTGRES-POOL-HEALTH-01): minimal
/// in-process registry of declared logical pool names mapped to
/// their <c>MaxPoolSize</c>. Constructed once at composition time
/// from the <c>PostgresPoolOptions</c> values that build the
/// <c>NpgsqlDataSource</c> instances. Read by
/// <c>PostgresPoolSnapshotProvider</c> at snapshot time so the
/// snapshot's <c>MaxConnections</c> field reflects the canonical
/// declared envelope rather than a guess.
/// </summary>
public sealed class PostgresPoolCatalog
{
    public IReadOnlyDictionary<string, int> MaxPoolSizeByName { get; }

    public PostgresPoolCatalog(IReadOnlyDictionary<string, int> maxPoolSizeByName)
    {
        ArgumentNullException.ThrowIfNull(maxPoolSizeByName);
        MaxPoolSizeByName = maxPoolSizeByName;
    }
}
