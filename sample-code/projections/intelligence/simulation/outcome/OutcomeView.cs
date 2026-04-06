namespace Whycespace.Projections.Intelligence.Simulation.Outcome;

public sealed record OutcomeView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
