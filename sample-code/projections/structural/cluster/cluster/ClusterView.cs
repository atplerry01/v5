namespace Whycespace.Projections.Structural.Cluster.Cluster;

public sealed record ClusterView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
