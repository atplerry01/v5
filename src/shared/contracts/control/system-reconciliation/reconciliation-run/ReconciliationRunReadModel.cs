namespace Whycespace.Shared.Contracts.Control.SystemReconciliation.ReconciliationRun;

public sealed record ReconciliationRunReadModel
{
    public Guid RunId { get; init; }
    public string Scope { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int ChecksProcessed { get; init; }
    public int DiscrepanciesFound { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public string? AbortReason { get; init; }
    public DateTimeOffset? AbortedAt { get; init; }
}
