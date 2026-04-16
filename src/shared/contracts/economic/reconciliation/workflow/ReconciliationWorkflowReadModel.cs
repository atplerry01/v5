namespace Whycespace.Shared.Contracts.Economic.Reconciliation.Workflow;

public sealed record ReconciliationWorkflowReadModel
{
    public Guid ProcessId { get; init; }
    public Guid? DiscrepancyId { get; init; }
    public string CurrentState { get; init; } = ReconciliationLifecycleState.Triggered.ToString();
    public string LastEvent { get; init; } = string.Empty;
    public Guid CorrelationId { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
