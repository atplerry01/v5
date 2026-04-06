namespace Whycespace.Projections.Decision.Risk.Threshold;

public sealed record ThresholdView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
