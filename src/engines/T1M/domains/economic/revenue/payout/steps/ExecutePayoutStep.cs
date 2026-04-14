using Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.State;
using Whycespace.Shared.Contracts.Economic.Vault.Account;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Revenue.Payout.Steps;

/// <summary>
/// Step 2: for each participant share, dispatches a DebitSliceCommand to the
/// SPV vault and a CreditSliceCommand to the participant vault. Enforces the
/// conservation invariant (total debit == total credit) after all shares
/// have been processed. Any single dispatch failure halts the workflow.
/// </summary>
public sealed class ExecutePayoutStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;

    public ExecutePayoutStep(ISystemIntentDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public string Name => PayoutExecutionSteps.ExecutePayout;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute VaultRoute = new("economic", "vault", "account");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<PayoutWorkflowState>()
            ?? throw new InvalidOperationException("PayoutWorkflowState not found in workflow context.");

        decimal totalDebit = 0m;
        decimal totalCredit = 0m;
        var emitted = new List<object>();

        foreach (var share in state.Shares)
        {
            var debit = new DebitSliceCommand(
                state.SpvVaultId,
                VaultSliceType.Slice1,
                share.Amount);

            var debitResult = await _dispatcher.DispatchAsync(debit, VaultRoute, cancellationToken);
            if (!debitResult.IsSuccess)
                return WorkflowStepResult.Failure(
                    debitResult.Error ?? $"Debit failed for participant {share.ParticipantId}.");

            totalDebit += share.Amount;
            emitted.AddRange(debitResult.EmittedEvents);

            var credit = new CreditSliceCommand(
                share.ParticipantVaultId,
                VaultSliceType.Slice1,
                share.Amount);

            var creditResult = await _dispatcher.DispatchAsync(credit, VaultRoute, cancellationToken);
            if (!creditResult.IsSuccess)
                return WorkflowStepResult.Failure(
                    creditResult.Error ?? $"Credit failed for participant {share.ParticipantId}.");

            totalCredit += share.Amount;
            emitted.AddRange(creditResult.EmittedEvents);
        }

        if (totalDebit != totalCredit)
            return WorkflowStepResult.Failure(
                $"Payout conservation violated: debit={totalDebit}, credit={totalCredit}.");

        state.CurrentStep = PayoutExecutionSteps.ExecutePayout;
        context.SetState(state);

        return WorkflowStepResult.Success(state.PayoutId, emitted);
    }
}
