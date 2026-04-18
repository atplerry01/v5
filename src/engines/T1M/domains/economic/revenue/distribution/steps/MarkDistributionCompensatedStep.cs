using Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.State;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.Steps;

/// <summary>
/// Phase 7 T7.3 — terminal distribution transition CompensationRequested →
/// Compensated. Requires the compensating journal id posted by the payout
/// compensation workflow (T7.4).
/// </summary>
public sealed class MarkDistributionCompensatedStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;

    public MarkDistributionCompensatedStep(ISystemIntentDispatcher dispatcher) =>
        _dispatcher = dispatcher;

    public string Name => DistributionCompensationSteps.MarkCompensated;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute DistributionRoute = new("economic", "revenue", "distribution");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<DistributionCompensationWorkflowState>()
            ?? throw new InvalidOperationException(
                "DistributionCompensationWorkflowState not found in workflow context.");

        if (string.IsNullOrWhiteSpace(state.CompensatingJournalId))
            return WorkflowStepResult.Failure(
                "CompensatingJournalId must be set by the ledger compensation step before marking compensated.");

        var command = new MarkDistributionCompensatedCommand(
            state.DistributionId,
            state.OriginalPayoutId,
            state.CompensatingJournalId);

        var result = await _dispatcher.DispatchSystemAsync(command, DistributionRoute, cancellationToken);
        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(
                result.Error ?? "MarkDistributionCompensated dispatch failed.");

        state.CurrentStep = DistributionCompensationSteps.MarkCompensated;
        context.SetState(state);

        return WorkflowStepResult.Success(state.DistributionId, result.EmittedEvents);
    }
}
