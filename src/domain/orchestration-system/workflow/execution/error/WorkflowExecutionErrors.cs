namespace Whycespace.Domain.OrchestrationSystem.Workflow.Execution;

public static class WorkflowExecutionErrors
{
    public const string NotRunning = "Workflow execution is not in Running state.";
    public const string WorkflowNameRequired = "Workflow name is required.";
    public const string CannotCompleteBeforeStarted = "Workflow execution cannot complete before it has started.";
    public const string CannotStepAfterCompleted = "Workflow execution cannot record a step after it has completed.";
    /// <summary>
    /// Pre-R3.A.3 constant preserved for backward source-compatibility
    /// with any consumers referencing the old name. Prefer
    /// <see cref="CannotResumeUnlessFailedOrSuspended"/> for new call
    /// sites — resume is now legal from BOTH <c>Failed</c> AND
    /// <c>Suspended</c> per R-WORKFLOW-SUSPEND-RESUME-GUARD-01.
    /// </summary>
    public const string CannotResumeUnlessFailed = "Workflow execution can only be resumed from the Failed or Suspended state.";

    /// <summary>
    /// R3.A.3 / R-WORKFLOW-SUSPEND-RESUME-GUARD-01 — canonical post-R3.A.3
    /// error for resume-guard failures.
    /// </summary>
    public const string CannotResumeUnlessFailedOrSuspended = "Workflow execution can only be resumed from the Failed or Suspended state.";

    /// <summary>
    /// R3.A.3 / R-WORKFLOW-SUSPEND-FACTORY-01 — guard failure when
    /// <c>WorkflowLifecycleEventFactory.Suspended</c> is invoked on
    /// an aggregate whose status is not <c>Running</c>. Suspending
    /// from NotStarted / Completed / Failed / Cancelled / Suspended
    /// is illegal.
    /// </summary>
    public const string CannotSuspendUnlessRunning = "Workflow execution can only be suspended from the Running state.";

    public const string CannotSkipSteps = "Workflow steps must be completed in order; the next expected step index does not match.";
    public const string StepNameRequired = "Step name is required.";

    /// <summary>
    /// R3.A.6 / R-WF-APPROVAL-04 — guard failure when
    /// <c>IWorkflowExecutionReplayService.CancelSuspendedAsync</c> is
    /// invoked on a workflow whose status is not <c>Suspended</c>. The
    /// approval-reject path intentionally enforces a stricter
    /// precondition than the generic cancel path (which accepts
    /// caller-driven cancellation from Running too) — only already-
    /// suspended workflows may be approval-rejected.
    /// </summary>
    public const string CannotRejectUnlessSuspended =
        "Workflow execution can only be approval-rejected from the Suspended state.";
}
