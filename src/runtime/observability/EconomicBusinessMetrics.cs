using System.Diagnostics.Metrics;
using Whycespace.Shared.Contracts.Observability;

namespace Whycespace.Runtime.Observability;

/// <summary>
/// Domain-level business metrics for the economic system. These metrics
/// reflect BUSINESS state, not just technical execution — they track
/// financial operations, enforcement actions, and settlement outcomes.
///
/// All instruments use System.Diagnostics.Metrics (OpenTelemetry-compatible).
/// Thread-safe via Counter/Histogram internals. DI-registered as the
/// singleton <see cref="IEconomicMetrics"/>; engines depend on the
/// interface only — not this implementation — so the engines layer does
/// not take a reference on Runtime (RUNTIME-LAYER-PURITY-01).
///
/// Meter: Whycespace.Economic.Business
/// </summary>
public sealed class EconomicBusinessMetrics : IEconomicMetrics
{
    private static readonly Meter EconomicMeter = new("Whycespace.Economic.Business", "1.0.0");

    // ── Settlement metrics ─────────────────────────────────────────────

    private static readonly Counter<long> SettlementSuccess =
        EconomicMeter.CreateCounter<long>("whyce.economic.settlement.success", "settlements", "Successfully initiated settlements");

    private static readonly Counter<long> SettlementFailure =
        EconomicMeter.CreateCounter<long>("whyce.economic.settlement.failure", "settlements", "Failed settlement initiations");

    // ── Ledger metrics ─────────────────────────────────────────────────

    private static readonly Counter<long> LedgerPostSuccess =
        EconomicMeter.CreateCounter<long>("whyce.economic.ledger.post.success", "postings", "Successful ledger journal postings");

    private static readonly Counter<long> LedgerPostFailure =
        EconomicMeter.CreateCounter<long>("whyce.economic.ledger.post.failure", "postings", "Failed ledger journal postings");

    // ── Recovery queue metrics ───────────────────────────────────────

    private static readonly Counter<long> RecoveryQueueEscalations =
        EconomicMeter.CreateCounter<long>("whyce.economic.recovery.queue.escalations", "events",
            "Workflow steps escalated to recovery queue after retry exhaustion");

    // ── Settlement reconciliation metrics ─────────────────────────────

    private static readonly Counter<long> SettlementReconciliationRequired =
        EconomicMeter.CreateCounter<long>("whyce.economic.settlement.reconciliation_required", "events",
            "Settlements requiring manual reconciliation due to internal/external mismatch");

    // ── Enforcement metrics ────────────────────────────────────────────
    // (restriction.triggered and enforcement.actions.count are emitted
    //  by ExecutionGuardMiddleware — they live on the
    //  Whycespace.Runtime.Enforcement meter. The metrics below capture
    //  workflow-level enforcement lifecycle outcomes.)

    private static readonly Counter<long> EnforcementWorkflowTriggered =
        EconomicMeter.CreateCounter<long>("whyce.economic.enforcement.workflow.triggered", "workflows", "Enforcement lifecycle workflows triggered");

    private static readonly Counter<long> EnforcementSanctionIssued =
        EconomicMeter.CreateCounter<long>("whyce.economic.enforcement.sanction.issued", "sanctions", "Sanctions issued via enforcement workflow");

    private static readonly Counter<long> EnforcementLockApplied =
        EconomicMeter.CreateCounter<long>("whyce.economic.enforcement.lock.applied", "locks", "System locks applied via enforcement workflow");

    // ── Transaction lifecycle metrics ──────────────────────────────────

    private static readonly Counter<long> TransactionLifecycleStarted =
        EconomicMeter.CreateCounter<long>("whyce.economic.transaction.lifecycle.started", "workflows", "Transaction lifecycle workflows started");

    private static readonly Counter<long> TransactionLifecycleCompleted =
        EconomicMeter.CreateCounter<long>("whyce.economic.transaction.lifecycle.completed", "workflows", "Transaction lifecycle workflows completed (ledger posted)");

    private static readonly Counter<long> TransactionLifecycleCompensated =
        EconomicMeter.CreateCounter<long>("whyce.economic.transaction.lifecycle.compensated", "workflows", "Transaction lifecycle workflows compensated (failed after partial progress)");

    private static readonly Histogram<double> TransactionAmounts =
        EconomicMeter.CreateHistogram<double>("whyce.economic.transaction.amount", "units", "Transaction amounts processed");

    // ── IEconomicMetrics implementation ─────────────────────────────────

    public void RecordSettlementSuccess(string currency, decimal amount)
    {
        SettlementSuccess.Add(1, new KeyValuePair<string, object?>("currency", currency));
        TransactionAmounts.Record((double)amount, new KeyValuePair<string, object?>("currency", currency), new("stage", "settlement"));
    }

    public void RecordSettlementFailure(string currency) =>
        SettlementFailure.Add(1, new KeyValuePair<string, object?>("currency", currency));

    public void RecordLedgerPostSuccess(string currency, decimal amount)
    {
        LedgerPostSuccess.Add(1, new KeyValuePair<string, object?>("currency", currency));
        TransactionAmounts.Record((double)amount, new KeyValuePair<string, object?>("currency", currency), new("stage", "ledger"));
    }

    public void RecordLedgerPostFailure(string currency) =>
        LedgerPostFailure.Add(1, new KeyValuePair<string, object?>("currency", currency));

    public void RecordEnforcementWorkflowTriggered(string severity) =>
        EnforcementWorkflowTriggered.Add(1, new KeyValuePair<string, object?>("severity", severity));

    public void RecordSanctionIssued(string severity) =>
        EnforcementSanctionIssued.Add(1, new KeyValuePair<string, object?>("severity", severity));

    public void RecordLockApplied(string severity) =>
        EnforcementLockApplied.Add(1, new KeyValuePair<string, object?>("severity", severity));

    public void RecordTransactionLifecycleStarted(string currency) =>
        TransactionLifecycleStarted.Add(1, new KeyValuePair<string, object?>("currency", currency));

    public void RecordTransactionLifecycleCompleted(string currency) =>
        TransactionLifecycleCompleted.Add(1, new KeyValuePair<string, object?>("currency", currency));

    public void RecordTransactionLifecycleCompensated(string reason) =>
        TransactionLifecycleCompensated.Add(1, new KeyValuePair<string, object?>("reason", reason));

    public void RecordSettlementReconciliationRequired(string currency) =>
        SettlementReconciliationRequired.Add(1, new KeyValuePair<string, object?>("currency", currency));

    public void RecordRecoveryQueueEscalation(string stepName) =>
        RecoveryQueueEscalations.Add(1, new KeyValuePair<string, object?>("step", stepName));

    // ── Phase 4 — control plane metrics ────────────────────────────────

    private static readonly Counter<long> LimitCheckSkipped =
        EconomicMeter.CreateCounter<long>("whyce.economic.limit.check.skipped", "checks",
            "Transactions where no active per-account limit was configured (control plane no-op)");

    private static readonly Counter<long> LimitCheckPassed =
        EconomicMeter.CreateCounter<long>("whyce.economic.limit.check.passed", "checks",
            "Transactions that passed the active per-account limit check");

    private static readonly Counter<long> LimitCheckBlocked =
        EconomicMeter.CreateCounter<long>("whyce.economic.limit.check.blocked", "checks",
            "Transactions blocked at the limit boundary before settlement / ledger");

    private static readonly Counter<long> FxLockSkipped =
        EconomicMeter.CreateCounter<long>("whyce.economic.fx.lock.skipped", "locks",
            "FXLockStep no-op (single-currency transaction)");

    private static readonly Counter<long> FxLocked =
        EconomicMeter.CreateCounter<long>("whyce.economic.fx.locked", "locks",
            "FX rate locked into transaction state for deterministic ledger posting");

    private static readonly Counter<long> FxLockMissing =
        EconomicMeter.CreateCounter<long>("whyce.economic.fx.lock.missing", "locks",
            "FX lock failed because no active rate was found for the requested pair");

    public void RecordLimitCheckSkipped(string currency) =>
        LimitCheckSkipped.Add(1, new KeyValuePair<string, object?>("currency", currency));

    public void RecordLimitCheckPassed(string currency, decimal amount)
    {
        LimitCheckPassed.Add(1, new KeyValuePair<string, object?>("currency", currency));
        TransactionAmounts.Record((double)amount, new KeyValuePair<string, object?>("currency", currency), new("stage", "limit"));
    }

    public void RecordLimitCheckBlocked(string currency) =>
        LimitCheckBlocked.Add(1, new KeyValuePair<string, object?>("currency", currency));

    public void RecordFxLockSkipped(string currency) =>
        FxLockSkipped.Add(1, new KeyValuePair<string, object?>("currency", currency));

    public void RecordFxLocked(string baseCurrency, string quoteCurrency, decimal rate) =>
        FxLocked.Add(1,
            new KeyValuePair<string, object?>("base", baseCurrency),
            new KeyValuePair<string, object?>("quote", quoteCurrency));

    public void RecordFxLockMissing(string baseCurrency, string quoteCurrency) =>
        FxLockMissing.Add(1,
            new KeyValuePair<string, object?>("base", baseCurrency),
            new KeyValuePair<string, object?>("quote", quoteCurrency));

    // ── Phase 5 — revenue pipeline metrics ─────────────────────────────

    private static readonly Counter<long> RevenueRecorded =
        EconomicMeter.CreateCounter<long>("whyce.economic.revenue.recorded", "events",
            "Revenue entries recorded against an SPV contract");

    private static readonly Counter<long> DistributionCreated =
        EconomicMeter.CreateCounter<long>("whyce.economic.revenue.distribution.created", "events",
            "Distribution aggregates created with per-participant share allocations");

    private static readonly Counter<long> PayoutExecuted =
        EconomicMeter.CreateCounter<long>("whyce.economic.revenue.payout.executed", "events",
            "Payout workflows completed across all participant shares (debit/credit pairs)");

    // ── Phase 5 — risk exposure lifecycle metrics ──────────────────────

    private static readonly Counter<long> RiskExposureOpened =
        EconomicMeter.CreateCounter<long>("whyce.economic.risk.exposure.opened", "events",
            "Risk exposures opened via CreateRiskExposure");

    public void RecordRevenueRecorded(string currency, decimal amount)
    {
        RevenueRecorded.Add(1, new KeyValuePair<string, object?>("currency", currency));
        TransactionAmounts.Record((double)amount, new KeyValuePair<string, object?>("currency", currency), new("stage", "revenue"));
    }

    public void RecordDistributionCreated(string spvId, decimal totalAmount, int shareCount) =>
        DistributionCreated.Add(1,
            new KeyValuePair<string, object?>("spv", spvId),
            new KeyValuePair<string, object?>("shares", shareCount));

    public void RecordPayoutExecuted(string spvId, decimal totalAmount, int shareCount) =>
        PayoutExecuted.Add(1,
            new KeyValuePair<string, object?>("spv", spvId),
            new KeyValuePair<string, object?>("shares", shareCount));

    public void RecordRiskExposureOpened(string exposureType, string currency) =>
        RiskExposureOpened.Add(1,
            new KeyValuePair<string, object?>("type", exposureType),
            new KeyValuePair<string, object?>("currency", currency));
}
