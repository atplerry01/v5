namespace Whycespace.Projections.Intelligence.Cost.CostVariance;

public sealed record CostVarianceView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
