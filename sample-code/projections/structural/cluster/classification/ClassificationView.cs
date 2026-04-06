namespace Whycespace.Projections.Structural.Cluster.Classification;

public sealed record ClassificationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
