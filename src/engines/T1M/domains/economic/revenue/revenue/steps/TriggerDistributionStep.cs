using Whycespace.Engines.T1M.Domains.Economic.Revenue.Revenue.State;
using Whycespace.Shared.Contracts.Economic.Revenue.Contract;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Revenue.Steps;

/// <summary>
/// T3.1 — single deterministic hop from Revenue → Distribution. Runs after
/// ApplyRevenueStep. Resolves contract allocations from the read side, derives
/// a deterministic DistributionId from the RevenueId, and dispatches
/// CreateDistributionCommand via DispatchSystemAsync (system-origin path).
///
/// Idempotency: the derived DistributionId is stable across retries, so the
/// downstream aggregate write is naturally de-duplicated by event-store
/// aggregate-id + version checks.
/// </summary>
public sealed class TriggerDistributionStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IContractAllocationsResolver _allocations;
    private readonly IIdGenerator _ids;

    public TriggerDistributionStep(
        ISystemIntentDispatcher dispatcher,
        IContractAllocationsResolver allocations,
        IIdGenerator ids)
    {
        _dispatcher = dispatcher;
        _allocations = allocations;
        _ids = ids;
    }

    public string Name => RevenueProcessingSteps.TriggerDistribution;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute DistributionRoute = new("economic", "revenue", "distribution");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<RevenueWorkflowState>()
            ?? throw new InvalidOperationException("RevenueWorkflowState not found in workflow context.");

        var allocations = await _allocations.ResolveAsync(state.ContractId, cancellationToken);
        if (allocations.Count == 0)
            return WorkflowStepResult.Failure(
                $"Contract {state.ContractId} resolved zero allocations — cannot create distribution.");

        var distributionId = _ids.Generate($"distribution|{state.RevenueId:N}");

        var command = new CreateDistributionCommand(
            distributionId,
            state.SpvId,
            state.Amount,
            allocations);

        var result = await _dispatcher.DispatchSystemAsync(command, DistributionRoute, cancellationToken);
        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(result.Error ?? "TriggerDistribution dispatch failed.");

        state.CurrentStep = RevenueProcessingSteps.TriggerDistribution;
        context.SetState(state);

        return WorkflowStepResult.Success(distributionId, result.EmittedEvents);
    }
}
