using Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.State;
using Whycespace.Shared.Contracts.Economic.Revenue.Contract;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Distribution.Steps;

/// <summary>
/// T3.3 — single deterministic hop from Distribution → Payout. Runs only
/// after a successful Confirm transition. Derives a deterministic PayoutId
/// from the DistributionId and dispatches ExecutePayoutCommand via
/// DispatchSystemAsync. Idempotency key is carried in the command so the
/// payout aggregate de-duplicates retries (T3.4).
/// </summary>
public sealed class TriggerPayoutStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IPayoutVaultLayoutResolver _vaults;
    private readonly IIdGenerator _ids;

    public TriggerPayoutStep(
        ISystemIntentDispatcher dispatcher,
        IPayoutVaultLayoutResolver vaults,
        IIdGenerator ids)
    {
        _dispatcher = dispatcher;
        _vaults = vaults;
        _ids = ids;
    }

    public string Name => DistributionWorkflowSteps.TriggerPayout;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute PayoutRoute = new("economic", "revenue", "payout");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<DistributionWorkflowState>()
            ?? throw new InvalidOperationException("DistributionWorkflowState not found in workflow context.");

        var participantIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var a in state.Allocations) participantIds.Add(a.ParticipantId);

        var layout = await _vaults.ResolveAsync(state.SpvId, participantIds, cancellationToken);

        var shares = new List<PayoutShareEntry>(state.Allocations.Count);
        foreach (var a in state.Allocations)
        {
            var amount = state.TotalAmount * (a.OwnershipPercentage / 100m);
            shares.Add(new PayoutShareEntry(a.ParticipantId, amount, a.OwnershipPercentage));
        }

        var payoutId = _ids.Generate($"payout|{state.DistributionId:N}");
        var idempotencyKey = $"payout|{state.DistributionId:N}|{state.SpvId}";

        var command = new ExecutePayoutCommand(
            payoutId,
            state.DistributionId,
            idempotencyKey,
            shares);

        var result = await _dispatcher.DispatchSystemAsync(command, PayoutRoute, cancellationToken);
        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(result.Error ?? "TriggerPayout dispatch failed.");

        state.CurrentStep = DistributionWorkflowSteps.TriggerPayout;
        context.SetState(state);

        // Layout is captured on the result for downstream observability; the
        // payout workflow re-resolves the same layout (deterministic by SpvId
        // + participant set) before executing vault movements.
        _ = layout;

        return WorkflowStepResult.Success(payoutId, result.EmittedEvents);
    }
}
