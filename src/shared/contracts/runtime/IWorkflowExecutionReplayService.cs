namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// Reconstructs workflow execution state from the event store. Lives in
/// shared/contracts/runtime so the runtime layer can depend on it without
/// referencing Whycespace.Domain.* (runtime.guard R-DOM-01). Concrete
/// implementation lives under src/engines/T1M/lifecycle/ where domain
/// references are permitted.
///
/// Replay MUST be event-driven only — no direct state restoration
/// (replay-determinism.guard).
/// </summary>
public interface IWorkflowExecutionReplayService
{
    Task<WorkflowExecutionReplayState?> ReplayAsync(Guid workflowExecutionId);

    /// <summary>
    /// Loads the workflow execution from the event store, asserts the
    /// aggregate is in the <b>Failed</b> state (failure-retry path
    /// only), and returns a freshly constructed
    /// <c>WorkflowExecutionResumedEvent</c> via
    /// <c>WorkflowLifecycleEventFactory.Resumed</c>. The aggregate is
    /// NOT mutated by this call (phase1.6-S1.2 /
    /// E-LIFECYCLE-FACTORY-CALL-SITE-01) — state change happens only
    /// when the persist pipeline replays the returned event through
    /// <c>Apply(WorkflowExecutionResumedEvent)</c>. The event is
    /// returned as <see cref="object"/> so the runtime contract stays
    /// domain-agnostic per runtime.guard R-DOM-01.
    ///
    /// <para>
    /// <b>R3.A.6 D8 XML-doc clarification:</b> although the factory's
    /// <c>Resumed(aggregate)</c> overload accepts both Failed and
    /// Suspended per R-WORKFLOW-SUSPEND-RESUME-GUARD-01, this service
    /// method enforces the stricter Failed-only precondition for the
    /// failure-retry caller. Suspended-resume for the human-approval
    /// path is handled by <see cref="ResumeWithApprovalAsync"/> per
    /// R-WF-APPROVAL-03, which reuses the same factory via the
    /// approval-carrier overload.
    /// </para>
    ///
    /// The dispatcher prepends this event onto the executing workflow's
    /// accumulated event list so it persists to the same aggregate stream
    /// before any new step events. Throws when the execution is missing,
    /// not in the Failed state, or otherwise cannot be resumed — there is
    /// no fake-resume path.
    /// </summary>
    Task<object> ResumeAsync(Guid workflowExecutionId);

    /// <summary>
    /// R3.A.6 / R-WF-APPROVAL-03 — approval-granted resume seam. Loads
    /// the workflow execution from the event store, asserts the
    /// aggregate is in the <b>Suspended</b> state AND the latest
    /// <c>WorkflowExecutionSuspendedEvent.Reason</c> starts with the
    /// canonical <c>human_approval</c> prefix, composes a
    /// <c>human_approval_granted:{signal}:{actor}[:{rationale}]</c>
    /// carrier, and returns a pre-baked
    /// <c>WorkflowExecutionResumedEvent</c> via the factory's
    /// approval-override overload. The <paramref name="approverIdentity"/>
    /// MUST be supplied by the caller from <c>CommandContext.ActorId</c>
    /// per R-WF-APPROVAL-07 — this method does NOT trust any actor
    /// segment in the preceding Suspended event's carrier text.
    ///
    /// Throws when the execution is missing, not Suspended, the latest
    /// Suspended event is not a human-approval suspend, or the approver
    /// identity is empty.
    /// </summary>
    Task<object> ResumeWithApprovalAsync(
        Guid workflowExecutionId, string approverIdentity, string? rationale = null);

    /// <summary>
    /// R3.A.6 / R-WF-APPROVAL-04 — approval-rejected cancel seam. Loads
    /// the workflow execution from the event store, asserts the
    /// aggregate is in the <b>Suspended</b> state AND the latest
    /// <c>WorkflowExecutionSuspendedEvent.Reason</c> starts with the
    /// canonical <c>human_approval</c> prefix, composes a
    /// <c>human_approval_rejected:{signal}:{actor}[:{rationale}]</c>
    /// carrier, and returns a pre-baked
    /// <c>WorkflowExecutionCancelledEvent</c> via
    /// <c>WorkflowLifecycleEventFactory.Cancelled</c>. The StepName on
    /// the Cancelled event is inherited from the latest Suspended
    /// event. The <paramref name="approverIdentity"/> MUST be supplied
    /// by the caller from <c>CommandContext.ActorId</c> per
    /// R-WF-APPROVAL-07.
    ///
    /// The workflow becomes terminal (Cancelled) after the persist
    /// pipeline replays this event; no engine re-entry occurs.
    /// </summary>
    Task<object> CancelSuspendedAsync(
        Guid workflowExecutionId, string approverIdentity, string? rationale = null);
}

/// <summary>
/// DTO carrying the minimal state needed to resume a workflow execution.
/// <see cref="NextStepIndex"/> is the cursor consumed by
/// <c>T1MWorkflowEngine.ExecuteAsync</c> via
/// <c>WorkflowExecutionContext.CurrentStepIndex</c>; it is the index of
/// the next step to run, derived deterministically from the count of
/// <c>WorkflowStepCompletedEvent</c> instances on the event stream
/// (unambiguous between "started, no steps done" and "step 0 completed",
/// which the aggregate's CurrentStepIndex cannot distinguish).
/// </summary>
public sealed record WorkflowExecutionReplayState(
    Guid WorkflowExecutionId,
    string WorkflowName,
    int NextStepIndex,
    string ExecutionHash,
    string Status,
    object? Payload,
    IReadOnlyDictionary<string, object?> StepOutputs);
