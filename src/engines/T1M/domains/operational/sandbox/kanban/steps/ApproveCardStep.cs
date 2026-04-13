using Whyce.Engines.T1M.Domains.Operational.Sandbox.Kanban.State;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Operational.Sandbox.Kanban;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Engines.T1M.Domains.Operational.Sandbox.Kanban.Steps;

/// <summary>
/// Step 3: Marks the card as approved by dispatching an UpdateKanbanCardCommand
/// that revises the card description to include approval metadata.
/// Does NOT access the domain directly — orchestrates via T2E only.
/// </summary>
public sealed class ApproveCardStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;

    public ApproveCardStep(ISystemIntentDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public string Name => CardApprovalSteps.Approve;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute KanbanRoute = new("operational", "sandbox", "kanban");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<CardApprovalState>()
            ?? throw new InvalidOperationException("CardApprovalState not found in workflow context.");

        var command = new UpdateKanbanCardCommand(
            Id: state.BoardId,
            CardId: state.CardId,
            Title: "[Approved]",
            Description: "Approved via card approval workflow.");

        var result = await _dispatcher.DispatchAsync(command, KanbanRoute, cancellationToken);

        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(result.Error ?? "Failed to approve card.");

        state.CurrentStep = CardApprovalSteps.Approve;
        state.Status = CardApprovalStatus.Approved;
        context.SetState(state);

        return WorkflowStepResult.Success(state.CardId, result.EmittedEvents);
    }
}
