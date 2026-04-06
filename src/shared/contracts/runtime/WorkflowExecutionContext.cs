namespace Whyce.Shared.Contracts.Runtime;

public sealed class WorkflowExecutionContext
{
    public required Guid WorkflowId { get; init; }
    public required Guid CorrelationId { get; init; }
    public required string WorkflowName { get; init; }
    public required object Payload { get; init; }
    public int CurrentStepIndex { get; set; }
    public string CurrentStep { get; set; } = string.Empty;
    public Dictionary<string, string> State { get; } = new();
    public Dictionary<string, object?> StepOutputs { get; } = new();
    public List<object> AccumulatedEvents { get; } = new();
    public object? WorkflowOutput { get; set; }
    public string ExecutionHash { get; set; } = string.Empty;
    public string? IdentityId { get; set; }
    public string? PolicyDecision { get; set; }
    public IWorkflowStepObserver? StepObserver { get; set; }
}
