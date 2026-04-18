using Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.State;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.Steps;

/// <summary>
/// T3.4 — opens the payout aggregate in the Requested state with a stable
/// idempotency key. Establishes the lifecycle row before any vault movement
/// so a crash mid-flight leaves a recoverable Requested record rather than
/// orphaned vault writes.
/// </summary>
public sealed class RequestPayoutStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;

    public RequestPayoutStep(ISystemIntentDispatcher dispatcher) => _dispatcher = dispatcher;

    public string Name => PayoutExecutionSteps.RequestPayout;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute PayoutRoute = new("economic", "revenue", "payout");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<PayoutWorkflowState>()
            ?? throw new InvalidOperationException("PayoutWorkflowState not found in workflow context.");

        var shares = new List<PayoutShareEntry>(state.Shares.Count);
        foreach (var s in state.Shares)
            shares.Add(new PayoutShareEntry(s.ParticipantId, s.Amount, s.Percentage));

        var command = new ExecutePayoutCommand(
            state.PayoutId,
            state.DistributionId,
            state.IdempotencyKey,
            shares);

        var result = await _dispatcher.DispatchSystemAsync(command, PayoutRoute, cancellationToken);
        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(result.Error ?? "RequestPayout dispatch failed.");

        state.CurrentStep = PayoutExecutionSteps.RequestPayout;
        context.SetState(state);

        return WorkflowStepResult.Success(state.PayoutId, result.EmittedEvents);
    }
}
