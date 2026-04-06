namespace Whyce.Runtime.WorkflowState;

public sealed class WorkflowState
{
    public required string WorkflowId { get; init; }
    public required string WorkflowName { get; init; }
    public int CurrentStepIndex { get; set; }
    public string ExecutionHash { get; set; } = string.Empty;
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Running;
    public string SerializedState { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
