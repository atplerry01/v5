namespace Whycespace.Projections.Structural.Humancapital.Performance;

public sealed record PerformanceView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
