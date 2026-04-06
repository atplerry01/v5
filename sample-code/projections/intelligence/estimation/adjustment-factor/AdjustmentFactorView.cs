namespace Whycespace.Projections.Intelligence.Estimation.AdjustmentFactor;

public sealed record AdjustmentFactorView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
