using Whyce.Engines.T1M.Domains.Operational.Sandbox.Kanban.State;
using Whyce.Shared.Contracts.Engine;
using Whyce.Shared.Contracts.Operational.Sandbox.Kanban.Card;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Engines.T1M.Domains.Operational.Sandbox.Kanban.Steps;

/// <summary>
/// Step 4 (Completion): Completes the card by dispatching a CompleteKanbanCardCommand.
/// Final step in the approval workflow — sets workflow output and terminal state.
/// Does NOT access the domain directly — orchestrates via T2E only.
/// </summary>
public sealed class CompleteCardStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;

    public CompleteCardStep(ISystemIntentDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public string Name => CardApprovalSteps.Complete;
    public WorkflowStepType StepType => WorkflowStepType.Completion;

    private static readonly DomainRoute KanbanRoute = new("operational", "sandbox", "kanban");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<CardApprovalState>()
            ?? throw new InvalidOperationException("CardApprovalState not found in workflow context.");

        var command = new CompleteKanbanCardCommand(
            Id: state.BoardId,
            CardId: state.CardId);

        var result = await _dispatcher.DispatchAsync(command, KanbanRoute, cancellationToken);

        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(result.Error ?? "Failed to complete card.");

        state.CurrentStep = CardApprovalSteps.Complete;
        state.Status = CardApprovalStatus.Completed;
        context.SetState(state);
        context.WorkflowOutput = state.CardId;

        return WorkflowStepResult.Success(state.CardId, result.EmittedEvents);
    }
}
