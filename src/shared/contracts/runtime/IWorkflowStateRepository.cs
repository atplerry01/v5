namespace Whyce.Shared.Contracts.Runtime;

public interface IWorkflowStateRepository
{
    Task SaveAsync(WorkflowStateRecord state);
    Task<WorkflowStateRecord?> GetAsync(string workflowId);
    Task UpdateAsync(WorkflowStateRecord state);
}

public sealed class WorkflowStateRecord
{
    public required string WorkflowId { get; init; }
    public required string WorkflowName { get; init; }
    public int CurrentStepIndex { get; set; }
    public string ExecutionHash { get; set; } = string.Empty;
    public string Status { get; set; } = "Running";
    public string SerializedState { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; set; }
}
