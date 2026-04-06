namespace Whycespace.Projections.Global.RegionPerformance;

public sealed record RegionPerformanceReadModel
{
    public required string Id { get; init; }
    public required string RegionId { get; init; }
    public long CommandsProcessed { get; init; }
    public long EventsPublished { get; init; }
    public decimal AvgCommandLatencyMs { get; init; }
    public decimal AvgReplicationLagMs { get; init; }
    public int ActiveConnections { get; init; }
    public bool IsHealthy { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public DateTimeOffset LastEventTimestamp { get; init; }
    public long LastEventVersion { get; init; }
}
