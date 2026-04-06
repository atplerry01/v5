namespace Whycespace.Projections.Intelligence.Simulation.Recommendation;

public sealed record RecommendationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
