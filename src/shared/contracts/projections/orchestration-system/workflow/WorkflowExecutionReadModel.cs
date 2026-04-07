namespace Whyce.Shared.Contracts.Projections.OrchestrationSystem.Workflow;

public sealed record WorkflowExecutionReadModel
{
    public required Guid WorkflowExecutionId { get; init; }
    public required string WorkflowName { get; init; }
    public int CurrentStepIndex { get; init; }
    public string ExecutionHash { get; init; } = string.Empty;
    public string Status { get; init; } = "Running";
    public string? FailedStepName { get; init; }
    public string? FailureReason { get; init; }
    public object? Payload { get; init; }
    // Init-only property reference; the dictionary contents themselves are
    // mutated in-place by WorkflowExecutionProjectionHandler when StepCompleted
    // arrives. Init-only prevents reassignment, not member mutation.
    public Dictionary<string, object?> StepOutputs { get; init; } = new();
}
