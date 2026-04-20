namespace Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

/// <summary>
/// Canonical workflow execution status.
///
/// Terminal states: <see cref="Completed"/>, <see cref="Failed"/>,
/// <see cref="Cancelled"/>. Failed is the only terminal state that
/// permits transition back to <see cref="Running"/> (via
/// <see cref="WorkflowLifecycleEventFactory.Resumed"/>). Cancelled
/// and Completed are strictly terminal — a new workflow execution is
/// required if the operator wants to re-run.
///
/// Enum integers are APPEND-ONLY: new values must be appended to
/// preserve stored event-store integrity. <see cref="Cancelled"/> was
/// added in R3.A.4 (2026-04-20) and occupies ordinal 4.
/// </summary>
public enum WorkflowExecutionStatus
{
    NotStarted,
    Running,
    Completed,
    Failed,
    /// <summary>
    /// R3.A.4 / R-WORKFLOW-CANCELLATION-EVENT-01 — workflow cancelled
    /// mid-flight by caller-driven <see cref="System.Threading.CancellationToken"/>.
    /// Terminal state; cannot resume (see
    /// <see cref="WorkflowLifecycleEventFactory.Resumed"/> guard).
    /// </summary>
    Cancelled,

    /// <summary>
    /// R3.A.3 / R-WORKFLOW-SUSPEND-EVENT-01 — workflow intentionally
    /// paused mid-flight to wait for an external signal (operator
    /// approval, external dependency, timer). NON-terminal: a
    /// suspended workflow can resume via
    /// <c>WorkflowExecutionResumedEvent</c> (see
    /// <c>WorkflowLifecycleEventFactory.Resumed</c> — the guard accepts
    /// BOTH <see cref="Failed"/> and <see cref="Suspended"/>).
    ///
    /// Ordinal 5 (appended post-R3.A.4's <see cref="Cancelled"/> at 4)
    /// — preserves stored enum integers under the append-only
    /// discipline.
    /// </summary>
    Suspended,
}
