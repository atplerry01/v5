namespace Whycespace.Projections.Global.LiveMonitoring;

public sealed record LiveMonitoringView
{
    public required string Id { get; init; }
    public required string RegionId { get; init; }
    public required string ActivationStatus { get; init; }
    public required bool HasCriticalAlerts { get; init; }
    public decimal AvgLatencyMs { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
