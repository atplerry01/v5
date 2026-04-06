namespace Whycespace.Projections.Intelligence.Index.PerformanceIndex;

public sealed record PerformanceIndexView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
