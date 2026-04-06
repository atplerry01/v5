namespace Whycespace.Projections.Intelligence.Estimation.AdjustmentFactor;

public sealed record AdjustmentFactorReadModel
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public DateTimeOffset LastEventTimestamp { get; init; }
    public long LastEventVersion { get; init; }
}
