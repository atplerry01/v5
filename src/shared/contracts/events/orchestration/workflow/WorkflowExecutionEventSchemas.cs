namespace Whycespace.Shared.Contracts.Events.Orchestration.Workflow;

public sealed record WorkflowExecutionStartedEventSchema(
    Guid AggregateId, string WorkflowName, object? Payload = null);

public sealed record WorkflowStepCompletedEventSchema(
    Guid AggregateId, int StepIndex, string StepName, string ExecutionHash, object? Output = null);

public sealed record WorkflowExecutionCompletedEventSchema(Guid AggregateId, string ExecutionHash);

public sealed record WorkflowExecutionFailedEventSchema(
    Guid AggregateId, string FailedStepName, string Reason);

public sealed record WorkflowExecutionResumedEventSchema(
    Guid AggregateId, string ResumedFromStepName, string PreviousFailureReason);

/// <summary>
/// R3.A.4 / R-WORKFLOW-CANCELLATION-EVENT-01 — on-wire schema for
/// <c>WorkflowExecutionCancelledEvent</c>. <c>StepName</c> is nullable
/// so cancellation-before-first-step scenarios are expressible.
/// <c>Reason</c> carries <c>caller_cancellation: TypeName: message</c>
/// for audit.
/// </summary>
public sealed record WorkflowExecutionCancelledEventSchema(
    Guid AggregateId, string? StepName, string Reason);

/// <summary>
/// R3.A.3 / R-WORKFLOW-SUSPEND-EVENT-01 — on-wire schema for
/// <c>WorkflowExecutionSuspendedEvent</c>. Symmetric shape with
/// <c>WorkflowExecutionCancelledEventSchema</c>; differs semantically
/// in that Suspended is non-terminal (resumable).
/// </summary>
public sealed record WorkflowExecutionSuspendedEventSchema(
    Guid AggregateId, string? StepName, string Reason);
