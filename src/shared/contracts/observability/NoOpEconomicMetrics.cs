namespace Whycespace.Shared.Contracts.Observability;

/// <summary>
/// No-op implementation of <see cref="IEconomicMetrics"/> for tests and
/// bootstrap scenarios where the real OTel-backed runtime implementation
/// is not wired. Safe to use anywhere; records nothing.
/// </summary>
public sealed class NoOpEconomicMetrics : IEconomicMetrics
{
    public static readonly NoOpEconomicMetrics Instance = new();

    public void RecordSettlementSuccess(string currency, decimal amount) { }
    public void RecordSettlementFailure(string currency) { }
    public void RecordLedgerPostSuccess(string currency, decimal amount) { }
    public void RecordLedgerPostFailure(string currency) { }
    public void RecordEnforcementWorkflowTriggered(string severity) { }
    public void RecordSanctionIssued(string severity) { }
    public void RecordLockApplied(string severity) { }
    public void RecordTransactionLifecycleStarted(string currency) { }
    public void RecordTransactionLifecycleCompleted(string currency) { }
    public void RecordTransactionLifecycleCompensated(string reason) { }
    public void RecordSettlementReconciliationRequired(string currency) { }
    public void RecordRecoveryQueueEscalation(string stepName) { }
    public void RecordLimitCheckSkipped(string currency) { }
    public void RecordLimitCheckPassed(string currency, decimal amount) { }
    public void RecordLimitCheckBlocked(string currency) { }
    public void RecordFxLockSkipped(string currency) { }
    public void RecordFxLocked(string baseCurrency, string quoteCurrency, decimal rate) { }
    public void RecordFxLockMissing(string baseCurrency, string quoteCurrency) { }
    public void RecordRevenueRecorded(string currency, decimal amount) { }
    public void RecordDistributionCreated(string spvId, decimal totalAmount, int shareCount) { }
    public void RecordPayoutExecuted(string spvId, decimal totalAmount, int shareCount) { }
    public void RecordRiskExposureOpened(string exposureType, string currency) { }
}
