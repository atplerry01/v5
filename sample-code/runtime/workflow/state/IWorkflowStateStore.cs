namespace Whycespace.Runtime.Workflow.State;

/// <summary>
/// Snapshot of a workflow instance for external persistence.
/// Versioned to support optimistic concurrency and replay safety.
/// </summary>
public sealed record WorkflowStateSnapshot
{
    public required Guid WorkflowId { get; init; }
    public required string CommandType { get; init; }
    public required string CorrelationId { get; init; }
    public required string ExecutionId { get; init; }
    public required WorkflowStatus Status { get; init; }
    public required int CurrentStepIndex { get; init; }
    public required int TotalSteps { get; init; }
    public required long Version { get; init; }
    public required IReadOnlyList<StepSnapshot> Steps { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}

public sealed record StepSnapshot
{
    public required string EngineCommandType { get; init; }
    public required StepStatus Status { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Persistent, externally durable workflow state store.
/// Supports versioned state, per-step checkpoints, and replay-safe recovery.
/// </summary>
public interface IWorkflowStateStore
{
    Task SaveAsync(WorkflowStateSnapshot snapshot, CancellationToken cancellationToken = default);
    Task<WorkflowStateSnapshot?> GetAsync(Guid workflowId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkflowStateSnapshot>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid workflowId, CancellationToken cancellationToken = default);
}
