using Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.State;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.Steps;

/// <summary>
/// Phase 7 T7.3 — transitions the DistributionAggregate from Paid|Failed to
/// CompensationRequested. Correlation to the original payout is mandatory.
/// </summary>
public sealed class RequestDistributionCompensationStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;

    public RequestDistributionCompensationStep(ISystemIntentDispatcher dispatcher) =>
        _dispatcher = dispatcher;

    public string Name => DistributionCompensationSteps.RequestCompensation;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute DistributionRoute = new("economic", "revenue", "distribution");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<DistributionCompensationWorkflowState>()
            ?? throw new InvalidOperationException(
                "DistributionCompensationWorkflowState not found in workflow context.");

        var command = new RequestDistributionCompensationCommand(
            state.DistributionId,
            state.OriginalPayoutId,
            state.Reason);

        var result = await _dispatcher.DispatchSystemAsync(command, DistributionRoute, cancellationToken);
        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(
                result.Error ?? "RequestDistributionCompensation dispatch failed.");

        state.CurrentStep = DistributionCompensationSteps.RequestCompensation;
        context.SetState(state);

        return WorkflowStepResult.Success(state.DistributionId, result.EmittedEvents);
    }
}
