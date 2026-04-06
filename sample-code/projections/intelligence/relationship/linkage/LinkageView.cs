namespace Whycespace.Projections.Intelligence.Relationship.Linkage;

public sealed record LinkageView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
