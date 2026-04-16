using Whycespace.Engines.T1M.Domains.Economic.Revenue.Revenue.State;
using Whycespace.Shared.Contracts.Economic.Revenue.Revenue;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Revenue.Steps;

public sealed class RecordRevenueStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;

    public RecordRevenueStep(ISystemIntentDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public string Name => RevenueProcessingSteps.RecordRevenue;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute RevenueRoute = new("economic", "revenue", "revenue");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<RevenueWorkflowState>()
            ?? throw new InvalidOperationException("RevenueWorkflowState not found in workflow context.");

        var command = new RecordRevenueCommand(
            state.RevenueId,
            state.SpvId,
            state.Amount,
            state.Currency,
            state.SourceRef);

        var result = await _dispatcher.DispatchAsync(command, RevenueRoute, cancellationToken);

        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(result.Error ?? "RecordRevenue dispatch failed.");

        state.CurrentStep = RevenueProcessingSteps.RecordRevenue;
        context.SetState(state);

        return WorkflowStepResult.Success(state.RevenueId, result.EmittedEvents);
    }
}
