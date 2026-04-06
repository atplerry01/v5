namespace Whycespace.Projections.Global.AnomalyDashboard;

public sealed record AnomalyDashboardReadModel
{
    public required string Id { get; init; }
    public required string RegionId { get; init; }
    public required string AnomalyType { get; init; }
    public required string Severity { get; init; }
    public int OccurrenceCount { get; init; }
    public string? LatestReason { get; init; }
    public bool IsResolved { get; init; }
    public DateTimeOffset FirstDetected { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public DateTimeOffset LastEventTimestamp { get; init; }
    public long LastEventVersion { get; init; }
}
