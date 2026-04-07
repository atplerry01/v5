namespace Whyce.Shared.Contracts.Events.OrchestrationSystem.Workflow;

public sealed record WorkflowExecutionStartedEventSchema(
    Guid AggregateId, string WorkflowName, object? Payload = null);

public sealed record WorkflowStepCompletedEventSchema(
    Guid AggregateId, int StepIndex, string StepName, string ExecutionHash, object? Output = null);

public sealed record WorkflowExecutionCompletedEventSchema(Guid AggregateId, string ExecutionHash);

public sealed record WorkflowExecutionFailedEventSchema(
    Guid AggregateId, string FailedStepName, string Reason);
