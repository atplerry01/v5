namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// R3.A.6 / R-WF-APPROVAL-04 — governed approval rejection for a
/// workflow currently in the Suspended-for-approval wait-state. On
/// acceptance, calls <see cref="IWorkflowExecutionReplayService.CancelSuspendedAsync"/>
/// which emits a
/// <see cref="Whycespace.Domain.OrchestrationSystem.Workflow.Execution.WorkflowExecutionCancelledEvent"/>
/// with a <c>human_approval_rejected:{signal}:{actor}:{reason?}</c>
/// carrier. Workflow becomes terminal (Cancelled); no engine
/// re-entry.
///
/// <para>
/// Rejects deterministically (canonical reason
/// <c>CannotRejectUnlessAwaitingApproval</c>) when the aggregate is
/// not in <c>Suspended</c> or the latest
/// <c>WorkflowExecutionSuspendedEvent.Reason</c> does not start with
/// the canonical <c>human_approval</c> prefix.
/// </para>
/// </summary>
public sealed record RejectWorkflowCommand(
    Guid WorkflowId,
    ApprovalDecisionPayload Decision) : IHasAggregateId
{
    public Guid AggregateId => WorkflowId;
}
