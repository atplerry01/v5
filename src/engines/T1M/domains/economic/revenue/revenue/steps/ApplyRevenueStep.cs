using Whycespace.Engines.T1M.Domains.Economic.Revenue.Revenue.State;
using Whycespace.Shared.Contracts.Economic.Vault.Account;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Revenue.Steps;

public sealed class ApplyRevenueStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;

    public ApplyRevenueStep(ISystemIntentDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public string Name => RevenueProcessingSteps.ApplyRevenue;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    // Vault routes use the raw classification "economic" (no -system suffix).
    private static readonly DomainRoute VaultRoute = new("economic", "vault", "account");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<RevenueWorkflowState>()
            ?? throw new InvalidOperationException("RevenueWorkflowState not found in workflow context.");

        var command = new ApplyRevenueCommand(
            state.VaultAccountId,
            state.Amount,
            state.Currency);

        var result = await _dispatcher.DispatchAsync(command, VaultRoute, cancellationToken);

        if (!result.IsSuccess)
            return WorkflowStepResult.Failure(result.Error ?? "ApplyRevenue dispatch failed.");

        state.CurrentStep = RevenueProcessingSteps.ApplyRevenue;
        context.SetState(state);

        return WorkflowStepResult.Success(state.RevenueId, result.EmittedEvents);
    }
}
