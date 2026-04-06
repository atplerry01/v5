namespace Whycespace.Runtime.EventFabric;

/// <summary>
/// Configuration for cross-region event replication.
/// Defines how events propagate across regional Kafka clusters.
/// Runtime-owned — no domain or engine references.
/// </summary>
public sealed record EventReplicationConfig
{
    public required string LocalRegionId { get; init; }
    public required IReadOnlyList<RegionalClusterConfig> RemoteClusters { get; init; }
    public required ReplicationStrategy Strategy { get; init; }
    public TimeSpan MaxReplicationLag { get; init; } = TimeSpan.FromSeconds(30);
}

public sealed record RegionalClusterConfig
{
    public required string RegionId { get; init; }
    public required string BootstrapServers { get; init; }
    public required bool IsActive { get; init; }
    public int Priority { get; init; }
}

public enum ReplicationStrategy
{
    /// <summary>All events replicated to all regions.</summary>
    FullMesh,
    /// <summary>Events replicated only to regions that subscribe to the BC.</summary>
    TopicBased,
    /// <summary>Events replicated based on jurisdiction routing rules.</summary>
    JurisdictionBased
}
