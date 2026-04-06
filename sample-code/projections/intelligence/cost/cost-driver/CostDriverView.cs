namespace Whycespace.Projections.Intelligence.Cost.CostDriver;

public sealed record CostDriverView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
