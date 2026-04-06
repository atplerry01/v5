namespace Whycespace.Projections.Intelligence.Simulation.Scenario;

public sealed record ScenarioView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
