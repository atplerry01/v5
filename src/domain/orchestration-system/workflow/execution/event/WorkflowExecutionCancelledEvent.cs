using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

/// <summary>
/// R3.A.4 / R-WORKFLOW-CANCELLATION-EVENT-01 — emitted when
/// <see cref="T1MWorkflowEngine"/> observes caller-driven cancellation
/// (request abort, host shutdown, explicit
/// <see cref="System.Threading.CancellationToken"/> cancel) and MUST
/// be raised BEFORE the propagating
/// <see cref="System.OperationCanceledException"/> re-throws to the
/// caller.
///
/// <para>
/// <b>Replay discipline:</b> the event is terminal — replaying a
/// stream ending in this event lands the aggregate at
/// <see cref="WorkflowExecutionStatus.Cancelled"/> and remains there.
/// Cancelled workflows cannot resume; a new workflow execution is
/// required if the operator wants to re-run.
/// </para>
///
/// <para>
/// <b>Field semantics:</b>
/// <list type="bullet">
///   <item><paramref name="StepName"/> — step that was in-flight when
///         cancellation arrived. Nullable so the event is usable in
///         edge-case scenarios (cancellation before first step).</item>
///   <item><paramref name="Reason"/> — canonical short tag
///         <c>caller_cancellation</c> plus a human-readable suffix
///         carrying the BCL exception type + message for audit. Not
///         a replay discriminator.</item>
/// </list>
/// </para>
/// </summary>
public sealed record WorkflowExecutionCancelledEvent(
    AggregateId AggregateId,
    string? StepName,
    string Reason) : DomainEvent;
