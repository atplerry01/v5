namespace Whycespace.Projections.Intelligence.Estimation.CostEstimate;

public sealed record CostEstimateView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
