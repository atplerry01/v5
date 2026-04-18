namespace Whycespace.Shared.Contracts.Observability;

/// <summary>
/// Abstraction over economic-system business metrics consumed by the
/// engines layer. Declared in Shared so engines can depend on it without
/// taking a reference on Runtime (which would invert the layer graph —
/// Runtime already references Engines).
///
/// The method surface is exactly the set of calls used by T1M workflow
/// steps today — no more, no less. Grow only when an engine step needs
/// a new signal.
/// </summary>
public interface IEconomicMetrics
{
    void RecordSettlementSuccess(string currency, decimal amount);
    void RecordSettlementFailure(string currency);

    void RecordLedgerPostSuccess(string currency, decimal amount);
    void RecordLedgerPostFailure(string currency);

    void RecordEnforcementWorkflowTriggered(string severity);
    void RecordSanctionIssued(string severity);
    void RecordLockApplied(string severity);

    void RecordTransactionLifecycleStarted(string currency);
    void RecordTransactionLifecycleCompleted(string currency);
    void RecordTransactionLifecycleCompensated(string reason);

    void RecordSettlementReconciliationRequired(string currency);
    void RecordRecoveryQueueEscalation(string stepName);

    // Phase 4 — transaction control plane signals.
    void RecordLimitCheckSkipped(string currency);
    void RecordLimitCheckPassed(string currency, decimal amount);
    void RecordLimitCheckBlocked(string currency);

    // Phase 4 — FX lock signals.
    void RecordFxLockSkipped(string currency);
    void RecordFxLocked(string baseCurrency, string quoteCurrency, decimal rate);
    void RecordFxLockMissing(string baseCurrency, string quoteCurrency);

    // Phase 5 — revenue pipeline signals.
    void RecordRevenueRecorded(string currency, decimal amount);
    void RecordDistributionCreated(string spvId, decimal totalAmount, int shareCount);
    void RecordPayoutExecuted(string spvId, decimal totalAmount, int shareCount);

    // Phase 5 — risk exposure lifecycle signals. Threshold breach detection
    // flows through the enforcement middleware meter
    // (whyce.enforcement.restriction.triggered) when an OPA rule matches;
    // this interface only carries the direct domain lifecycle signals.
    void RecordRiskExposureOpened(string exposureType, string currency);
}
