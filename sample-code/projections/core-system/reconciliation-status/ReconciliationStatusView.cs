namespace Whycespace.Projections.CoreSystem.ReconciliationStatus;

public sealed record ReconciliationStatusView
{
    public required string Id { get; init; }
    public required string ScopeType { get; init; }
    public required string TargetSystem { get; init; }
    public required string Status { get; init; }
    public required bool AllConsistent { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
