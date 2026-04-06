namespace Whycespace.Projections.Intelligence.Cost.CostBenchmark;

public sealed record CostBenchmarkView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
