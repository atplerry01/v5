namespace Whycespace.Projections.Structural.Cluster.Topology;

public sealed record TopologyView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
