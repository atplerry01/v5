using Whycespace.Engines.T1M.Domains.Economic.Transaction.State;
using Whycespace.Shared.Contracts.Economic.Transaction.Settlement;
using Whycespace.Shared.Contracts.Economic.Transaction.Transaction;
using Whycespace.Shared.Contracts.Economic.Transaction.Workflow;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T1M.Domains.Economic.Transaction.Steps;

/// <summary>
/// Step 3: Initiate settlement — triggers external settlement for the
/// transaction amount via the configured provider.
///
/// COMPENSATION: If settlement initiation fails, the step dispatches a
/// FailTransactionCommand to prevent the transaction from remaining in
/// a half-open state. This ensures no transaction persists without either
/// completing the full lifecycle or being explicitly failed.
/// </summary>
public sealed class InitiateSettlementStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IEconomicMetrics _metrics;

    public InitiateSettlementStep(
        ISystemIntentDispatcher dispatcher,
        IEconomicMetrics metrics)
    {
        _dispatcher = dispatcher;
        _metrics = metrics;
    }

    public string Name => TransactionLifecycleSteps.InitiateSettlement;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute SettlementRoute = new("economic", "transaction", "settlement");
    private static readonly DomainRoute TransactionRoute = new("economic", "transaction", "transaction");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<TransactionLifecycleWorkflowState>()
            ?? throw new InvalidOperationException("TransactionLifecycleWorkflowState not found in workflow context.");

        var command = new InitiateSettlementCommand(
            state.SettlementId,
            state.Amount,
            state.Currency,
            state.TransactionId.ToString(),
            state.SettlementProvider);

        var result = await _dispatcher.DispatchSystemAsync(command, SettlementRoute, cancellationToken);
        if (!result.IsSuccess)
        {
            _metrics.RecordSettlementFailure(state.Currency);

            // COMPENSATION: Fail the transaction so it does not remain
            // in a half-open initiated state without settlement.
            var failCommand = new FailTransactionCommand(
                state.TransactionId,
                $"Settlement initiation failed: {result.Error ?? "unknown"}. " +
                "Transaction failed as compensation.",
                state.InitiatedAt);

            await _dispatcher.DispatchSystemAsync(failCommand, TransactionRoute, cancellationToken);
            _metrics.RecordTransactionLifecycleCompensated("settlement_failure");

            return WorkflowStepResult.Failure(result.Error ?? "InitiateSettlement dispatch failed.");
        }

        _metrics.RecordSettlementSuccess(state.Currency, state.Amount);

        // SETTLEMENT FINALITY: Mark as Pending — the external provider has
        // accepted the initiation but has not yet confirmed finality. If a
        // downstream step (ledger posting) fails and triggers compensation,
        // the compensation logic must check this before assuming reversal
        // is safe.
        state.SettlementFinality = SettlementFinalityRecord.Pending(state.SettlementId);
        state.CurrentStep = TransactionLifecycleSteps.InitiateSettlement;
        context.SetState(state);

        return WorkflowStepResult.Success(state.SettlementId, result.EmittedEvents);
    }
}
