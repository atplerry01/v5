namespace Whycespace.Projections.Intelligence.Relationship.Graph;

public sealed record GraphView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
