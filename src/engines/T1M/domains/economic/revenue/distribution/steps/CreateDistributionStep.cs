using Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.State;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.Steps;

public sealed class CreateDistributionStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IEconomicMetrics _metrics;

    public CreateDistributionStep(ISystemIntentDispatcher dispatcher, IEconomicMetrics metrics)
    {
        _dispatcher = dispatcher;
        _metrics = metrics;
    }

    public string Name => DistributionWorkflowSteps.Create;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute DistributionRoute = new("economic", "revenue", "distribution");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<DistributionWorkflowState>()
            ?? throw new InvalidOperationException("DistributionWorkflowState not found in workflow context.");

        var command = new CreateDistributionCommand(
            state.DistributionId,
            state.SpvId,
            state.TotalAmount,
            state.Allocations);

        var result = await _dispatcher.DispatchSystemAsync(command, DistributionRoute, cancellationToken);

        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(result.Error ?? "CreateDistribution dispatch failed.");

        _metrics.RecordDistributionCreated(state.SpvId, state.TotalAmount, state.Allocations.Count);

        state.CurrentStep = DistributionWorkflowSteps.Create;
        context.SetState(state);

        return WorkflowStepResult.Success(state.DistributionId, result.EmittedEvents);
    }
}
