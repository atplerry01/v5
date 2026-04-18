using Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.State;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.Steps;

/// <summary>
/// T3.4 — transitions the payout aggregate from Requested → Executed once
/// vault movements have completed and conservation has been verified.
/// </summary>
public sealed class MarkPayoutExecutedStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;

    public MarkPayoutExecutedStep(ISystemIntentDispatcher dispatcher) => _dispatcher = dispatcher;

    public string Name => PayoutExecutionSteps.MarkPayoutExecuted;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute PayoutRoute = new("economic", "revenue", "payout");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<PayoutWorkflowState>()
            ?? throw new InvalidOperationException("PayoutWorkflowState not found in workflow context.");

        var command = new MarkPayoutExecutedCommand(state.PayoutId);

        var result = await _dispatcher.DispatchSystemAsync(command, PayoutRoute, cancellationToken);
        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(result.Error ?? "MarkPayoutExecuted dispatch failed.");

        state.CurrentStep = PayoutExecutionSteps.MarkPayoutExecuted;
        context.SetState(state);

        return WorkflowStepResult.Success(state.PayoutId, result.EmittedEvents);
    }
}
