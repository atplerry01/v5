using Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.State;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.Steps;

/// <summary>
/// Phase 7 T7.3 — terminal payout transition CompensationRequested →
/// Compensated. Runs after the compensating journal has been posted
/// successfully. Compensated is terminal and irreversible (T7.2).
/// </summary>
public sealed class MarkPayoutCompensatedStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;

    public MarkPayoutCompensatedStep(ISystemIntentDispatcher dispatcher) =>
        _dispatcher = dispatcher;

    public string Name => PayoutCompensationSteps.MarkCompensated;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute PayoutRoute = new("economic", "revenue", "payout");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<PayoutCompensationWorkflowState>()
            ?? throw new InvalidOperationException(
                "PayoutCompensationWorkflowState not found in workflow context.");

        if (state.CompensatingJournalId == Guid.Empty)
            return WorkflowStepResult.Failure(
                "CompensatingJournalId must be set by the ledger compensation step before marking compensated.");

        var command = new MarkPayoutCompensatedCommand(
            state.PayoutId,
            state.CompensatingJournalId.ToString());

        var result = await _dispatcher.DispatchSystemAsync(command, PayoutRoute, cancellationToken);
        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(
                result.Error ?? "MarkPayoutCompensated dispatch failed.");

        state.CurrentStep = PayoutCompensationSteps.MarkCompensated;
        context.SetState(state);

        return WorkflowStepResult.Success(state.PayoutId, result.EmittedEvents);
    }
}
