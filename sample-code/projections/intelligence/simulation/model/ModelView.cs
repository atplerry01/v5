namespace Whycespace.Projections.Intelligence.Simulation.Model;

public sealed record ModelView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
