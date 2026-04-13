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
    /// aggregate is in the Failed state, and returns a freshly constructed
    /// <c>WorkflowExecutionResumedEvent</c> via
    /// <c>WorkflowLifecycleEventFactory.Resumed</c>. The aggregate is NOT
    /// mutated by this call (phase1.6-S1.2 / E-LIFECYCLE-FACTORY-CALL-SITE-01)
    /// — state change happens only when the persist pipeline replays the
    /// returned event through <c>Apply(WorkflowExecutionResumedEvent)</c>.
    /// The event is returned as <see cref="object"/> so the runtime contract
    /// stays domain-agnostic per runtime.guard R-DOM-01.
    ///
    /// The dispatcher prepends this event onto the executing workflow's
    /// accumulated event list so it persists to the same aggregate stream
    /// before any new step events. Throws when the execution is missing,
    /// not in the Failed state, or otherwise cannot be resumed — there is
    /// no fake-resume path.
    /// </summary>
    Task<object> ResumeAsync(Guid workflowExecutionId);
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
