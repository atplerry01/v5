namespace Whycespace.Projections.Structural.Cluster.Continuity;

public sealed record ContinuityView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
