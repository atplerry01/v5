namespace Whycespace.Runtime.EventFabric;

/// <summary>
/// Latency-aware event routing for cross-region dispatch.
/// Selects the optimal target cluster based on measured latency
/// and replication lag. Runtime infrastructure — no domain logic.
/// </summary>
public sealed class LatencyAwareRouter
{
    private readonly EventReplicationConfig _config;

    public LatencyAwareRouter(EventReplicationConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Returns the best remote cluster for a given event,
    /// favoring active clusters with lowest priority number (highest priority).
    /// </summary>
    public RegionalClusterConfig? ResolveTarget(string targetRegionId)
    {
        return _config.RemoteClusters
            .Where(c => c.RegionId == targetRegionId && c.IsActive)
            .OrderBy(c => c.Priority)
            .FirstOrDefault();
    }

    /// <summary>
    /// Returns all active remote clusters for full-mesh replication.
    /// </summary>
    public IReadOnlyList<RegionalClusterConfig> GetActiveRemoteClusters() =>
        _config.RemoteClusters.Where(c => c.IsActive).ToList();

    /// <summary>
    /// Determines if the event should be replicated based on strategy.
    /// </summary>
    public bool ShouldReplicate(string eventType, string targetRegionId)
    {
        return _config.Strategy switch
        {
            ReplicationStrategy.FullMesh => true,
            ReplicationStrategy.TopicBased => _config.RemoteClusters.Any(c => c.RegionId == targetRegionId && c.IsActive),
            ReplicationStrategy.JurisdictionBased => _config.RemoteClusters.Any(c => c.RegionId == targetRegionId && c.IsActive),
            _ => false
        };
    }
}
