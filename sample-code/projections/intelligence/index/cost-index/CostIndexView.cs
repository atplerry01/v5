namespace Whycespace.Projections.Intelligence.Index.CostIndex;

public sealed record CostIndexView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
