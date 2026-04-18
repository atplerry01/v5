using Whycespace.Engines.T1M.Domains.Economic.Transaction.State;
using Whycespace.Shared.Contracts.Economic.Transaction.Limit;
using Whycespace.Shared.Contracts.Economic.Transaction.Workflow;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Engines.T1M.Domains.Economic.Transaction.Steps;

/// <summary>
/// Phase 4 T4.1 / T4.2 — control-plane gate. Runs AFTER InitiateTransactionStep
/// and BEFORE InitiateSettlementStep + PostToLedgerStep so no transaction can
/// reach the ledger without passing the per-account limit check.
///
/// Flow:
///   1. Resolve the active limit for FromAccountId + Currency from the read side.
///   2. If no active limit exists → step succeeds (account has no per-account
///      ceiling; the EnforcementGuard middleware still applies). The control
///      plane runs unconditionally; this step's outcome is conditional on
///      whether a limit is configured.
///   3. If a limit exists → DispatchSystemAsync(CheckLimitCommand). The handler
///      loads LimitAggregate.Check, which raises LimitCheckedEvent on success
///      or throws LimitErrors.LimitExceeded / ConcurrencyConflict on breach.
///      Either domain failure surfaces back here as a non-success
///      CommandResult; the step returns Failure so the workflow halts before
///      InitiateSettlementStep / PostToLedgerStep ever runs.
///
/// Hard-block guarantee: a returned Failure halts the workflow per the
/// existing engine contract. No partial state mutation occurs because
/// settlement and ledger steps simply do not execute.
/// </summary>
public sealed class CheckLimitStep : IWorkflowStep
{
    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly ILimitResolver _limitResolver;
    private readonly IEconomicMetrics _metrics;

    public CheckLimitStep(
        ISystemIntentDispatcher dispatcher,
        ILimitResolver limitResolver,
        IEconomicMetrics metrics)
    {
        _dispatcher = dispatcher;
        _limitResolver = limitResolver;
        _metrics = metrics;
    }

    public string Name => TransactionLifecycleSteps.CheckLimit;
    public WorkflowStepType StepType => WorkflowStepType.Validation;

    private static readonly DomainRoute LimitRoute = new("economic", "transaction", "limit");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<TransactionLifecycleWorkflowState>()
            ?? throw new InvalidOperationException("TransactionLifecycleWorkflowState not found in workflow context.");

        var resolution = await _limitResolver.ResolveAsync(
            state.FromAccountId, state.Currency, cancellationToken);

        if (resolution is null)
        {
            // No active limit defined for this account/currency. Step is a
            // no-op for this transaction; control plane still ran.
            _metrics.RecordLimitCheckSkipped(state.Currency);
            state.CurrentStep = TransactionLifecycleSteps.CheckLimit;
            context.SetState(state);
            return WorkflowStepResult.Success(state.TransactionId);
        }

        var command = new CheckLimitCommand(
            resolution.LimitId,
            state.TransactionId,
            state.Amount,
            state.InitiatedAt);

        var result = await _dispatcher.DispatchSystemAsync(command, LimitRoute, cancellationToken);
        if (!result.IsSuccess)
        {
            _metrics.RecordLimitCheckBlocked(state.Currency);
            return WorkflowStepResult.Failure(
                result.Error
                ?? $"CheckLimit failed for transaction {state.TransactionId} on account {state.FromAccountId}.");
        }

        _metrics.RecordLimitCheckPassed(state.Currency, state.Amount);

        state.CurrentStep = TransactionLifecycleSteps.CheckLimit;
        state.LimitId = resolution.LimitId;
        context.SetState(state);

        return WorkflowStepResult.Success(state.TransactionId, result.EmittedEvents);
    }
}
