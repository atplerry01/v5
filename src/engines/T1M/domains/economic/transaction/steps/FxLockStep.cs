using Whycespace.Engines.T1M.Domains.Economic.Transaction.State;
using Whycespace.Shared.Contracts.Economic.Exchange.Rate;
using Whycespace.Shared.Contracts.Economic.Transaction.Workflow;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Transaction.Steps;

/// <summary>
/// Phase 4 T4.4 — locks a deterministic FX rate onto the workflow state
/// before ledger posting. Runs AFTER InitiateSettlementStep and BEFORE
/// PostToLedgerStep so the ledger entries can carry the rate that was in
/// force when the transaction crossed the control plane.
///
/// Behavior:
///   * <c>state.FxBaseCurrency</c> null OR equal to <c>state.Currency</c>
///     → no FX conversion needed; step succeeds as a no-op.
///   * Otherwise → resolve the active rate for (FxBaseCurrency, Currency)
///     as of <c>state.InitiatedAt</c> and snapshot it onto
///     <see cref="TransactionLifecycleWorkflowState.FxLock"/>. PostToLedgerStep
///     consumes the lock when computing converted entries.
///   * If a cross-currency transaction has no active rate → return Failure.
///     Without a deterministic rate, ledger posting cannot proceed.
/// </summary>
public sealed class FxLockStep : IWorkflowStep
{
    private readonly IExchangeRateResolver _rates;
    private readonly IEconomicMetrics _metrics;

    public FxLockStep(IExchangeRateResolver rates, IEconomicMetrics metrics)
    {
        _rates = rates;
        _metrics = metrics;
    }

    public string Name => TransactionLifecycleSteps.FxLock;
    public WorkflowStepType StepType => WorkflowStepType.Validation;

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<TransactionLifecycleWorkflowState>()
            ?? throw new InvalidOperationException("TransactionLifecycleWorkflowState not found in workflow context.");

        var noConversion = string.IsNullOrWhiteSpace(state.FxBaseCurrency)
            || string.Equals(state.FxBaseCurrency, state.Currency, StringComparison.OrdinalIgnoreCase);

        if (noConversion)
        {
            _metrics.RecordFxLockSkipped(state.Currency);
            state.CurrentStep = TransactionLifecycleSteps.FxLock;
            context.SetState(state);
            return WorkflowStepResult.Success(state.TransactionId);
        }

        var rate = await _rates.ResolveAsync(
            state.FxBaseCurrency!, state.Currency, state.InitiatedAt, cancellationToken);

        if (rate is null)
        {
            _metrics.RecordFxLockMissing(state.FxBaseCurrency!, state.Currency);
            return WorkflowStepResult.Failure(
                $"FxLock failed: no active rate for {state.FxBaseCurrency} → {state.Currency} as of {state.InitiatedAt:O}. " +
                "Cross-currency transactions cannot proceed without a deterministic rate.");
        }

        state.FxLock = new LockedExchangeRate(
            rate.RateId,
            rate.BaseCurrency,
            rate.QuoteCurrency,
            rate.Rate,
            state.InitiatedAt);
        state.CurrentStep = TransactionLifecycleSteps.FxLock;
        context.SetState(state);

        _metrics.RecordFxLocked(rate.BaseCurrency, rate.QuoteCurrency, rate.Rate);

        return WorkflowStepResult.Success(state.TransactionId);
    }
}
