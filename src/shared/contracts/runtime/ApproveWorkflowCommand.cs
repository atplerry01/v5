namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// R3.A.6 / R-WF-APPROVAL-03 — governed approval decision for a
/// workflow currently in the Suspended-for-approval wait-state. On
/// acceptance, re-enters the existing <see cref="IWorkflowExecutionReplayService.ResumeAsync"/>
/// seam via a factory overload that composes a
/// <c>human_approval_granted:{signal}:{actor}</c> carrier onto the
/// emitted <see cref="Whycespace.Domain.OrchestrationSystem.Workflow.Execution.WorkflowExecutionResumedEvent"/>.
/// Workflow re-enters <c>T1MWorkflowEngine.ExecuteAsync</c> at
/// <c>WorkflowExecutionReplayState.NextStepIndex</c>.
///
/// <para>
/// Rejects deterministically (canonical reason
/// <c>CannotApproveUnlessAwaitingApproval</c>) when the aggregate is
/// not in <c>Suspended</c> or the latest
/// <c>WorkflowExecutionSuspendedEvent.Reason</c> does not start with
/// the canonical <c>human_approval</c> prefix.
/// </para>
/// </summary>
public sealed record ApproveWorkflowCommand(
    Guid WorkflowId,
    ApprovalDecisionPayload Decision) : IHasAggregateId
{
    public Guid AggregateId => WorkflowId;
}
