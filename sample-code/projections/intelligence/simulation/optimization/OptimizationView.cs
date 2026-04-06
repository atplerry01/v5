namespace Whycespace.Projections.Intelligence.Simulation.Optimization;

public sealed record OptimizationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
