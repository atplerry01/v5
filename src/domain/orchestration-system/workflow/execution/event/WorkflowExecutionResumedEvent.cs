using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

/// <summary>
/// Raised when a previously failed workflow execution is resumed. Carries the
/// step name from which execution will continue and the failure reason that
/// preceded the resume so the audit trail is self-contained on replay.
/// Deterministic: every field is supplied by the caller from upstream context.
/// </summary>
public sealed record WorkflowExecutionResumedEvent(
    AggregateId AggregateId,
    string ResumedFromStepName,
    string PreviousFailureReason) : DomainEvent;
