using Whycespace.Engines.T1M.Domains.Operational.Sandbox.Kanban.State;
using Whycespace.Engines.T1M.Domains.Operational.Sandbox.Kanban.Workflows;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Operational.Sandbox.Kanban.Steps;

/// <summary>
/// Step 1: Validates that the CardApprovalIntent carries all required
/// coordinates before any T2E commands are dispatched.
/// Creates and stores the typed workflow state for subsequent steps.
/// </summary>
public sealed class ValidateCardStep : IWorkflowStep
{
    public string Name => CardApprovalSteps.Validate;
    public WorkflowStepType StepType => WorkflowStepType.Validation;

    public Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.Payload is not CardApprovalIntent intent)
            return Task.FromResult(
                WorkflowStepResult.Failure("Payload is not a valid CardApprovalIntent."));

        if (intent.BoardId == Guid.Empty)
            return Task.FromResult(
                WorkflowStepResult.Failure("BoardId is required."));

        if (intent.CardId == Guid.Empty)
            return Task.FromResult(
                WorkflowStepResult.Failure("CardId is required."));

        if (intent.ReviewListId == Guid.Empty)
            return Task.FromResult(
                WorkflowStepResult.Failure("ReviewListId is required."));

        if (string.IsNullOrWhiteSpace(intent.UserId))
            return Task.FromResult(
                WorkflowStepResult.Failure("UserId is required."));

        var state = CardApprovalState.Create(
            intent.BoardId,
            intent.CardId,
            intent.FromListId,
            intent.ReviewListId);
        context.SetState(state);

        return Task.FromResult(
            WorkflowStepResult.Success(intent));
    }
}
