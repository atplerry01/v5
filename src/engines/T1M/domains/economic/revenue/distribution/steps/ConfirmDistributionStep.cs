using Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.State;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.Steps;

/// <summary>
/// T3.2 / T3.3 — moves the DistributionAggregate from Created → Confirmed.
/// Only Confirmed distributions trigger payout in TriggerPayoutStep.
/// </summary>
public sealed class ConfirmDistributionStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;

    public ConfirmDistributionStep(ISystemIntentDispatcher dispatcher) => _dispatcher = dispatcher;

    public string Name => DistributionWorkflowSteps.Confirm;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute DistributionRoute = new("economic", "revenue", "distribution");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<DistributionWorkflowState>()
            ?? throw new InvalidOperationException("DistributionWorkflowState not found in workflow context.");

        var command = new ConfirmDistributionCommand(state.DistributionId);

        var result = await _dispatcher.DispatchSystemAsync(command, DistributionRoute, cancellationToken);
        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(result.Error ?? "ConfirmDistribution dispatch failed.");

        state.CurrentStep = DistributionWorkflowSteps.Confirm;
        context.SetState(state);

        return WorkflowStepResult.Success(state.DistributionId, result.EmittedEvents);
    }
}
