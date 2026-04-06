namespace Whycespace.Projections.Intelligence.Estimation.PriceEstimate;

public sealed record PriceEstimateView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
