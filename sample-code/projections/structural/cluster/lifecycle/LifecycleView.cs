namespace Whycespace.Projections.Structural.Cluster.Lifecycle;

public sealed record LifecycleView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
