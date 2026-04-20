using Whycespace.Engines.T1M.Domains.Economic.Transaction.State;
using Whycespace.Shared.Contracts.Economic.Ledger.Journal;
using Whycespace.Shared.Contracts.Economic.Transaction.Transaction;
using Whycespace.Shared.Contracts.Economic.Transaction.Workflow;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Observability;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T1M.Domains.Economic.Transaction.Steps;

/// <summary>
/// Step 4 (TERMINAL): Post balanced journal entries to the ledger — produces
/// the double-entry debit/credit pair that closes the transaction lifecycle.
///
/// LEDGER GUARANTEE: This is the REQUIRED terminal step. If ledger posting
/// fails after retry, the step dispatches a FailTransactionCommand as
/// compensation to prevent partial financial state. No transaction can
/// complete without a successful ledger posting.
///
/// DETERMINISM: Entry IDs are derived from IIdGenerator using the
/// JournalId as seed, ensuring replay-stable identifiers ($9).
///
/// RETRY ESCALATION MODEL:
///   Attempt 1: immediate dispatch.
///   Attempt 2: 200ms backoff.
///   Attempt 3: 400ms backoff.
///   If a permanent failure is detected (validation, policy, not-found),
///   retry is short-circuited and compensation fires immediately.
///   If all attempts fail with transient errors and a recovery queue is
///   available, the step escalates to the recovery queue for deferred
///   retry. Compensation only fires when no recovery path exists.
/// </summary>
public sealed class PostToLedgerStep : IWorkflowStep
{
    private static readonly RetryPolicy Retry = new() { MaxAttempts = 3, InitialDelayMs = 200 };

    private readonly ISystemIntentDispatcher _dispatcher;
    private readonly IIdGenerator _idGenerator;
    private readonly IEconomicMetrics _metrics;
    private readonly IRecoveryQueue? _recoveryQueue;
    private readonly IRetryExecutor? _retryExecutor;

    public PostToLedgerStep(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IEconomicMetrics metrics)
        : this(dispatcher, idGenerator, metrics, null, null) { }

    public PostToLedgerStep(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IEconomicMetrics metrics,
        IRecoveryQueue? recoveryQueue)
        : this(dispatcher, idGenerator, metrics, recoveryQueue, null) { }

    // R2.A.2 (2026-04-19) — canonical retry executor injected. When null,
    // the step falls back to the legacy in-line loop to preserve behavior
    // in unit tests or hosts that haven't registered the executor yet.
    // The host DI registration in CoreComposition makes this non-null by
    // default.
    public PostToLedgerStep(
        ISystemIntentDispatcher dispatcher,
        IIdGenerator idGenerator,
        IEconomicMetrics metrics,
        IRecoveryQueue? recoveryQueue,
        IRetryExecutor? retryExecutor)
    {
        _dispatcher = dispatcher;
        _idGenerator = idGenerator;
        _metrics = metrics;
        _recoveryQueue = recoveryQueue;
        _retryExecutor = retryExecutor;
    }

    public string Name => TransactionLifecycleSteps.PostToLedger;
    public WorkflowStepType StepType => WorkflowStepType.Command;

    private static readonly DomainRoute LedgerRoute = new("economic", "ledger", "journal");
    private static readonly DomainRoute TransactionRoute = new("economic", "transaction", "transaction");

    public async Task<WorkflowStepResult> ExecuteAsync(
        WorkflowExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        var state = context.GetState<TransactionLifecycleWorkflowState>()
            ?? throw new InvalidOperationException("TransactionLifecycleWorkflowState not found in workflow context.");

        // Deterministic entry IDs — replay-stable via IIdGenerator ($9).
        var debitEntryId = _idGenerator.Generate($"{state.JournalId:N}:debit");
        var creditEntryId = _idGenerator.Generate($"{state.JournalId:N}:credit");

        // Phase 6 T6.1 — FX rate snapshot. When FxLockStep locked a rate
        // for this cross-currency transaction, stamp the (RateId, Rate)
        // tuple onto every journal entry so the ledger projection never
        // needs to re-resolve exchange state. Null on single-currency
        // transactions (FxLockStep no-op path).
        var fxRateId = state.FxLock?.RateId;
        var fxRate = state.FxLock?.Rate;

        var entries = new List<JournalEntryInput>
        {
            new(debitEntryId,  state.FromAccountId, state.Amount, state.Currency, "Debit",  fxRateId, fxRate),
            new(creditEntryId, state.ToAccountId,   state.Amount, state.Currency, "Credit", fxRateId, fxRate)
        };

        var command = new PostJournalEntriesCommand(
            state.LedgerId,
            state.JournalId,
            entries);

        // R2.A.2 — canonical retry via IRetryExecutor. Replay-deterministic
        // (clock + seeded jitter), category-driven eligibility (R-RETRY-CAT-01),
        // bounded attempts (R-RETRY-CAP-01), full evidence trail
        // (R-RETRY-EVIDENCE-01). Legacy fallback path preserved for hosts /
        // tests that don't inject the executor.
        string? lastError;
        bool isPermanent;

        if (_retryExecutor is not null)
        {
            var retryCtx = new RetryOperationContext
            {
                OperationId = $"{state.JournalId:N}:post-ledger",
                Policy = Retry,
                OperationName = "post-to-ledger"
            };

            var retryResult = await _retryExecutor.ExecuteAsync<CommandResult>(
                retryCtx,
                async (attempt, ct) =>
                {
                    var result = await _dispatcher.DispatchSystemAsync(command, LedgerRoute, ct);
                    if (result.IsSuccess)
                        return RetryStepResult<CommandResult>.Success(result);

                    // Prefer the canonical category set by R1 Batch 3.5 middleware
                    // wiring. Fall back to ExecutionFailure (retryable default)
                    // for legacy callers that haven't been uplifted yet.
                    var category = result.FailureCategory ?? RuntimeFailureCategory.ExecutionFailure;
                    return RetryStepResult<CommandResult>.Failure(
                        category,
                        result.Error ?? "unknown");
                },
                cancellationToken);

            if (retryResult.Outcome == RetryOutcome.Success && retryResult.Value is { IsSuccess: true } successResult)
            {
                _metrics.RecordLedgerPostSuccess(state.Currency, state.Amount);
                _metrics.RecordTransactionLifecycleCompleted(state.Currency);

                state.CurrentStep = TransactionLifecycleSteps.PostToLedger;
                context.SetState(state);
                return WorkflowStepResult.Success(state.JournalId, successResult.EmittedEvents);
            }

            lastError = retryResult.FinalError;
            isPermanent = retryResult.Outcome == RetryOutcome.PermanentFailure;
        }
        else
        {
            // Legacy in-line retry loop — preserved for unit tests / host
            // configurations without IRetryExecutor registered. Matches
            // pre-R2.A.2 behavior verbatim. To be deleted once every host
            // registers the executor.
            lastError = null;
            isPermanent = false;

            for (var attempt = 1; attempt <= Retry.MaxAttempts; attempt++)
            {
                var delayMs = Retry.GetDelayMs(attempt);
                if (delayMs > 0)
                    await Task.Delay(delayMs, cancellationToken);

                var result = await _dispatcher.DispatchSystemAsync(command, LedgerRoute, cancellationToken);
                if (result.IsSuccess)
                {
                    _metrics.RecordLedgerPostSuccess(state.Currency, state.Amount);
                    _metrics.RecordTransactionLifecycleCompleted(state.Currency);

                    state.CurrentStep = TransactionLifecycleSteps.PostToLedger;
                    context.SetState(state);
                    return WorkflowStepResult.Success(state.JournalId, result.EmittedEvents);
                }

                lastError = result.Error;

                if (RetryPolicy.IsPermanentFailure(lastError))
                {
                    isPermanent = true;
                    break;
                }

                if (cancellationToken.IsCancellationRequested)
                    break;
            }
        }

        _metrics.RecordLedgerPostFailure(state.Currency);

        // ESCALATION: Before compensating, try to publish to the recovery
        // queue. The recovery worker will retry with longer intervals (30s,
        // 60s, 120s) which can ride out infrastructure blips that the
        // in-line retry could not. Only compensate when recovery is not
        // available or when the failure is permanent.
        //
        // R2.A.2 — `isPermanent` now derives from the retry executor's
        // Outcome (PermanentFailure when a non-retryable category short-
        // circuits) rather than the deprecated string-based heuristic.
        if (_recoveryQueue is not null && !isPermanent)
        {
            var entry = new RecoveryEntry(
                EntryId: state.JournalId,
                WorkflowName: TransactionLifecycleWorkflowNames.Lifecycle,
                StepName: TransactionLifecycleSteps.PostToLedger,
                SerializedState: System.Text.Json.JsonSerializer.Serialize(state),
                LastError: lastError ?? "unknown",
                AttemptCount: Retry.MaxAttempts,
                FailedAt: state.InitiatedAt,
                Classification: "economic",
                Context: "ledger",
                Domain: "journal");

            var published = await _recoveryQueue.PublishAsync(entry, cancellationToken);
            if (published)
            {
                _metrics.RecordRecoveryQueueEscalation("ledger_post");

                return WorkflowStepResult.Failure(
                    $"PostJournalEntries failed after {Retry.MaxAttempts} attempts " +
                    $"(last error: {lastError ?? "unknown"}). Escalated to recovery queue. " +
                    "Transaction will be resolved by recovery worker.");
            }
        }

        // COMPENSATION: No recovery path available or permanent failure.
        await CompensateTransactionAsync(state, lastError, cancellationToken);
        _metrics.RecordTransactionLifecycleCompensated("ledger_failure");

        return WorkflowStepResult.Failure(
            $"LEDGER GUARANTEE VIOLATION: PostJournalEntries failed after {Retry.MaxAttempts} attempts " +
            $"(last error: {lastError ?? "unknown"}). Transaction {state.TransactionId} " +
            "has been failed as compensation. Manual reconciliation required.");
    }

    private async Task CompensateTransactionAsync(
        TransactionLifecycleWorkflowState state,
        string? ledgerError,
        CancellationToken cancellationToken)
    {
        // SETTLEMENT FINALITY CHECK: If the settlement was initiated with
        // an external provider, compensation creates a dangerous mismatch:
        //   Internal=Failed + External=Pending/Confirmed
        // Flag this for mandatory reconciliation.
        var requiresReconciliation = state.SettlementFinality?.RequiresReconciliation ?? false;
        var reconciliationNote = requiresReconciliation
            ? " RECONCILIATION REQUIRED: Settlement was externally initiated — " +
              "verify provider state before closing."
            : "";

        var failCommand = new FailTransactionCommand(
            state.TransactionId,
            $"Ledger posting failed after {Retry.MaxAttempts} retries: {ledgerError ?? "unknown"}. " +
            $"Compensatory fail to prevent partial financial state.{reconciliationNote}",
            state.InitiatedAt);

        // Best-effort compensation — if this also fails, the workflow
        // failure surfaces to the orchestrator for manual resolution.
        await _dispatcher.DispatchSystemAsync(failCommand, TransactionRoute, cancellationToken);

        if (requiresReconciliation)
        {
            // Fail the settlement explicitly so the settlement projection
            // reflects the internal failure. The reconciliation process
            // will compare this against the external provider state.
            var failSettlement = new Shared.Contracts.Economic.Transaction.Settlement.FailSettlementCommand(
                state.SettlementId,
                $"Compensatory settlement failure: ledger posting failed. " +
                "External settlement may have been committed — reconciliation required.");

            await _dispatcher.DispatchSystemAsync(failSettlement,
                new DomainRoute("economic", "transaction", "settlement"), cancellationToken);

            _metrics.RecordSettlementReconciliationRequired(state.Currency);
        }
    }
}
