using Whyce.Engines.T1M.Domains.Operational.Sandbox.Kanban.State;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Operational.Sandbox.Kanban.Card;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Engines.T1M.Domains.Operational.Sandbox.Kanban.Steps;

/// <summary>
/// Step 2: Moves the card from its current list to the Review list
/// by dispatching a MoveKanbanCardCommand through the runtime.
/// Does NOT access the domain directly — orchestrates via T2E only.
/// </summary>
public sealed class MoveToReviewStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;

    public MoveToReviewStep(ISystemIntentDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public string Name => CardApprovalSteps.MoveToReview;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute KanbanRoute = new("operational", "sandbox", "kanban");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<CardApprovalState>()
            ?? throw new InvalidOperationException("CardApprovalState not found in workflow context.");

        var command = new MoveKanbanCardCommand(
            Id: state.BoardId,
            CardId: state.CardId,
            FromListId: state.FromListId,
            ToListId: state.ReviewListId,
            NewPosition: 0);

        var result = await _dispatcher.DispatchAsync(command, KanbanRoute, cancellationToken);

        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(result.Error ?? "Failed to move card to review list.");

        state.CurrentStep = CardApprovalSteps.MoveToReview;
        state.Status = CardApprovalStatus.InReview;
        context.SetState(state);

        return WorkflowStepResult.Success(state.CardId, result.EmittedEvents);
    }
}
