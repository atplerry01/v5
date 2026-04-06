namespace Whycespace.Projections.Global.RegionPerformance;

public sealed record RegionPerformanceView
{
    public required string Id { get; init; }
    public required string RegionId { get; init; }
    public required bool IsHealthy { get; init; }
    public decimal AvgCommandLatencyMs { get; init; }
    public decimal AvgReplicationLagMs { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
