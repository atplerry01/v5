namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// R3.A.6 / R-WF-APPROVAL-06 — payload for
/// <see cref="ApproveWorkflowCommand"/> and
/// <see cref="RejectWorkflowCommand"/>. Carries the operator-supplied
/// rationale and (optionally, deferred per R3.A.6 D2) an explicit
/// approval key for multi-approval workflows.
///
/// <para>
/// <b>Actor sourcing:</b> this payload deliberately does NOT carry an
/// approver identity field. Authoritative approver identity is sourced
/// from <c>CommandContext.Actor</c> (runtime identity middleware output)
/// per R-WF-APPROVAL-07. Any actor information rendered into the
/// lifecycle event's Reason/PreviousFailureReason carrier text is a
/// non-authoritative observability mirror.
/// </para>
/// </summary>
public sealed record ApprovalDecisionPayload(
    string Rationale,
    string? ApprovalKey = null);
