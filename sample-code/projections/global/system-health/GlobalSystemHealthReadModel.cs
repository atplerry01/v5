namespace Whycespace.Projections.Global.SystemHealth;

public sealed record GlobalSystemHealthReadModel
{
    public required string Id { get; init; }
    public required string RegionId { get; init; }
    public required string SystemName { get; init; }
    public required string Status { get; init; }
    public bool IsHealthy { get; init; }
    public int ActiveAggregates { get; init; }
    public decimal AvgLatencyMs { get; init; }
    public long EventsProcessed { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public DateTimeOffset LastEventTimestamp { get; init; }
    public long LastEventVersion { get; init; }
}
