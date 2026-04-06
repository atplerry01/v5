namespace Whycespace.Projections.Global.LatencyTracking;

public sealed record LatencyTrackingReadModel
{
    public required string Id { get; init; }
    public required string RegionId { get; init; }
    public required string CommandType { get; init; }
    public decimal P50LatencyMs { get; init; }
    public decimal P95LatencyMs { get; init; }
    public decimal P99LatencyMs { get; init; }
    public long SampleCount { get; init; }
    public bool IsWithinBudget { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public DateTimeOffset LastEventTimestamp { get; init; }
    public long LastEventVersion { get; init; }
}
