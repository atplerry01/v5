namespace Whycespace.Projections.Business.Portfolio.Benchmark;

public sealed record BenchmarkView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
