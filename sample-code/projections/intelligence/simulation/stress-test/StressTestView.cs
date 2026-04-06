namespace Whycespace.Projections.Intelligence.Simulation.StressTest;

public sealed record StressTestView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
