using Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.State;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.Steps;

public sealed class CreateDistributionStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;

    public CreateDistributionStep(ISystemIntentDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
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

        var result = await _dispatcher.DispatchAsync(command, DistributionRoute, cancellationToken);

        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(result.Error ?? "CreateDistribution dispatch failed.");

        state.CurrentStep = DistributionWorkflowSteps.Create;
        context.SetState(state);

        return WorkflowStepResult.Success(state.DistributionId, result.EmittedEvents);
    }
}
