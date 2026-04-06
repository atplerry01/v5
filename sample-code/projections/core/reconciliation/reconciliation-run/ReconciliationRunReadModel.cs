namespace Whycespace.Projections.Core.Reconciliation.ReconciliationRun;

public sealed record ReconciliationRunReadModel
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public DateTimeOffset LastEventTimestamp { get; init; }
    public long LastEventVersion { get; init; }
}
