namespace Whycespace.Projections.Structural.Cluster.Subcluster;

public sealed record SubclusterView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
