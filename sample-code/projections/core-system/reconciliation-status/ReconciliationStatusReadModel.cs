namespace Whycespace.Projections.CoreSystem.ReconciliationStatus;

public sealed record ReconciliationStatusReadModel
{
    public required string Id { get; init; }
    public required string ScopeType { get; init; }
    public required string TargetSystem { get; init; }
    public required string Status { get; init; }
    public int TotalChecks { get; init; }
    public int FailedChecks { get; init; }
    public bool AllConsistent { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public DateTimeOffset LastEventTimestamp { get; init; }
    public long LastEventVersion { get; init; }
}
