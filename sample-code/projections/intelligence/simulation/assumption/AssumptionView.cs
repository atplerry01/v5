namespace Whycespace.Projections.Intelligence.Simulation.Assumption;

public sealed record AssumptionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
