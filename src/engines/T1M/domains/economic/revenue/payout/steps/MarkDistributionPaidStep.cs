using Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.State;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.Steps;

/// <summary>
/// T3.2 closure — once payout has executed and the ledger journal has been
/// posted, transitions the upstream DistributionAggregate from Confirmed → Paid.
/// </summary>
public sealed class MarkDistributionPaidStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;

    public MarkDistributionPaidStep(ISystemIntentDispatcher dispatcher) => _dispatcher = dispatcher;

    public string Name => PayoutExecutionSteps.MarkDistributionPaid;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute DistributionRoute = new("economic", "revenue", "distribution");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<PayoutWorkflowState>()
            ?? throw new InvalidOperationException("PayoutWorkflowState not found in workflow context.");

        var command = new MarkDistributionPaidCommand(state.DistributionId, state.PayoutId);

        var result = await _dispatcher.DispatchSystemAsync(command, DistributionRoute, cancellationToken);
        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(result.Error ?? "MarkDistributionPaid dispatch failed.");

        state.CurrentStep = PayoutExecutionSteps.MarkDistributionPaid;
        context.SetState(state);

        return WorkflowStepResult.Success(state.DistributionId, result.EmittedEvents);
    }
}
