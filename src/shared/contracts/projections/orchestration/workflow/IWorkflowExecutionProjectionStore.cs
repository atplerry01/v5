namespace Whycespace.Shared.Contracts.Projections.Orchestration.Workflow;

/// <summary>
/// Read-model persistence for workflow executions.
/// R4.B extends the contract with bounded list queries so the admin/operator
/// surface can inspect canonical lifecycle states. <paramref name="limit"/>
/// MUST be capped at an implementation-defined ceiling (1000) to bound
/// operator-query blast radius.
/// </summary>
public interface IWorkflowExecutionProjectionStore
{
    Task<WorkflowExecutionReadModel?> GetAsync(Guid workflowExecutionId);
    Task UpsertAsync(WorkflowExecutionReadModel model);

    /// <summary>
    /// R4.B — list executions matching a canonical Status value
    /// (Running / Suspended / Cancelled / Failed / Completed).
    /// Results ordered by workflow-name ascending.
    /// </summary>
    Task<IReadOnlyList<WorkflowExecutionReadModel>> ListByStatusAsync(
        string status,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// R4.B — list executions with optional filters on status, workflow name,
    /// and approval-state. Null filter = no constraint.
    /// </summary>
    Task<IReadOnlyList<WorkflowExecutionReadModel>> ListAsync(
        string? status = null,
        string? workflowName = null,
        string? approvalState = null,
        int limit = 100,
        CancellationToken cancellationToken = default);
}
