namespace Whycespace.Projections.Global.LiveMonitoring;

public sealed record LiveMonitoringReadModel
{
    public required string Id { get; init; }
    public required string RegionId { get; init; }
    public required string ActivationStatus { get; init; }
    public required int TrafficPercent { get; init; }
    public long CommandsProcessed { get; init; }
    public long AnomaliesDetected { get; init; }
    public long GovernanceEscalations { get; init; }
    public decimal AvgLatencyMs { get; init; }
    public bool HasCriticalAlerts { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public DateTimeOffset LastEventTimestamp { get; init; }
    public long LastEventVersion { get; init; }
}
