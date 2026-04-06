namespace Whycespace.Projections.Intelligence.Economic.Simulation;

public sealed record SimulationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
