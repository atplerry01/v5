namespace Whycespace.Projections.Intelligence.Simulation.Comparison;

public sealed record ComparisonView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
