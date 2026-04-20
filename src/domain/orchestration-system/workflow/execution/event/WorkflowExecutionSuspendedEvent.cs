using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

/// <summary>
/// R3.A.3 / R-WORKFLOW-SUSPEND-EVENT-01 — emitted when a workflow is
/// intentionally paused mid-flight to await an external signal. The
/// canonical use cases are human-approval wait-state, external
/// dependency wait, and timer-based delay. NON-terminal — a
/// <see cref="WorkflowExecutionResumedEvent"/> moves the aggregate
/// back to <see cref="WorkflowExecutionStatus.Running"/>.
///
/// <para>
/// <b>Replay discipline:</b> symmetric with
/// <see cref="WorkflowExecutionCancelledEvent"/>: <paramref name="StepName"/>
/// and <paramref name="Reason"/> are audit/observability data, not
/// replay discriminators. Two streams that land at the same end
/// state (via resume-chain or not) are replay-equivalent.
/// </para>
///
/// <para>
/// <b>Scope boundary:</b> R3.A.3 lands the event surface + aggregate
/// state machine. The engine does not yet expose a caller-initiated
/// suspend trigger — a <c>WorkflowSuspendCommand</c> or step-yield
/// API is R4 operator-surface scope. This event type is ready to
/// consume when those layers ship.
/// </para>
/// </summary>
public sealed record WorkflowExecutionSuspendedEvent(
    AggregateId AggregateId,
    string? StepName,
    string Reason) : DomainEvent;
