namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// R3.A.6 / R-WF-APPROVAL-02 — canonical reasons for
/// <see cref="ApproveWorkflowCommand"/> / <see cref="RejectWorkflowCommand"/>
/// precondition failures. Lives in the shared-contracts runtime
/// surface so the runtime dispatcher can emit them without referencing
/// <c>Whycespace.Domain.*</c> (runtime.guard R-DOM-01).
///
/// <para>
/// <b>Carrier prefix:</b> the canonical signal prefix is
/// <c>human_approval</c>. Suspended workflows awaiting an approval
/// decision carry this prefix in their latest
/// <c>WorkflowExecutionSuspendedEvent.Reason</c>; non-approval
/// suspensions (timer, external dependency) do NOT and MUST be
/// rejected by Approve/Reject handlers.
/// </para>
/// </summary>
public static class WorkflowApprovalErrors
{
    /// <summary>Canonical Reason prefix for approval wait-state / grant / reject carriers.</summary>
    public const string HumanApprovalPrefix = "human_approval";

    public const string HumanApprovalGrantedPrefix = "human_approval_granted";

    public const string HumanApprovalRejectedPrefix = "human_approval_rejected";

    public const string CannotApproveUnlessAwaitingApproval =
        "Approve is rejected: workflow is not in Suspended state with a human_approval signal.";

    public const string CannotRejectUnlessAwaitingApproval =
        "Reject is rejected: workflow is not in Suspended state with a human_approval signal.";

    public const string CannotRejectUnlessSuspended =
        "Workflow can only be approval-cancelled from the Suspended state.";

    public const string ApprovalRationaleRequired =
        "ApprovalDecisionPayload.Rationale is required.";
}
