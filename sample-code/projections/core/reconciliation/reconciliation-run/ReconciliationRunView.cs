namespace Whycespace.Projections.Core.Reconciliation.ReconciliationRun;

public sealed record ReconciliationRunView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
