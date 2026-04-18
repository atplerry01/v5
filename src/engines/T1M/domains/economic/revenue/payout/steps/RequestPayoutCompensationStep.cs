using Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.State;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.Steps;

/// <summary>
/// Phase 7 T7.3 — transitions the PayoutAggregate from Executed|Failed to
/// CompensationRequested. This is the entry point of the payout
/// compensation workflow; the next step posts the compensating ledger
/// journal (T7.4) and the terminal step marks the payout Compensated.
/// </summary>
public sealed class RequestPayoutCompensationStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;

    public RequestPayoutCompensationStep(ISystemIntentDispatcher dispatcher) =>
        _dispatcher = dispatcher;

    public string Name => PayoutCompensationSteps.RequestCompensation;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute PayoutRoute = new("economic", "revenue", "payout");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<PayoutCompensationWorkflowState>()
            ?? throw new InvalidOperationException(
                "PayoutCompensationWorkflowState not found in workflow context.");

        var command = new RequestPayoutCompensationCommand(state.PayoutId, state.Reason);

        var result = await _dispatcher.DispatchSystemAsync(command, PayoutRoute, cancellationToken);
        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(
                result.Error ?? "RequestPayoutCompensation dispatch failed.");

        state.CurrentStep = PayoutCompensationSteps.RequestCompensation;
        context.SetState(state);

        return WorkflowStepResult.Success(state.PayoutId, result.EmittedEvents);
    }
}
