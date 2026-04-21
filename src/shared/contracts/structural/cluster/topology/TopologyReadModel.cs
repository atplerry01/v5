namespace Whycespace.Shared.Contracts.Structural.Cluster.Topology;

public sealed record TopologyReadModel
{
    public Guid TopologyId { get; init; }
    public Guid ClusterReference { get; init; }
    public string TopologyName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
