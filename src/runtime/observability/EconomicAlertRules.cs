using System.Diagnostics.Metrics;

namespace Whycespace.Runtime.Observability;

/// <summary>
/// Alert rule definitions for the economic system. Each rule maps a
/// business metric to a threshold, severity, and runbook action.
///
/// These rules are evaluated by the observability stack (Prometheus +
/// Alertmanager / Grafana Alerting) via the OTel metrics exported by
/// <see cref="EconomicBusinessMetrics"/> and
/// <see cref="Whycespace.Runtime.Middleware.Execution.ExecutionGuardMiddleware"/>.
///
/// This class exposes the rules as structured data so that:
///   1. Alert configuration is code-reviewed alongside the system.
///   2. Integration tests can verify that all referenced metrics exist.
///   3. The host can publish rules to the alerting backend at startup.
///
/// RULE SEMANTICS:
///   • All thresholds are per-minute rates unless noted otherwise.
///   • Critical alerts page on-call immediately.
///   • Warning alerts create a ticket within 4 hours.
///   • Info alerts are dashboard-only (no notification).
/// </summary>
public static class EconomicAlertRules
{
    public static IReadOnlyList<AlertRule> All { get; } = new AlertRule[]
    {
        // ── Financial integrity alerts (Critical — page immediately) ──

        new(
            Name: "LedgerPostFailureRate",
            MetricName: "whyce.economic.ledger.post.failure",
            Threshold: 1,
            WindowSeconds: 60,
            Severity: AlertSeverity.Critical,
            Summary: "Ledger journal posting failed — financial state may be inconsistent",
            Runbook: "1. Check Postgres projection_economic_ledger connectivity. " +
                     "2. Verify journal aggregate stream is writable. " +
                     "3. Check recovery queue for escalated entries. " +
                     "4. If entries exist in recovery queue, monitor for resolution. " +
                     "5. If no recovery queue: check transaction projection for compensated transactions requiring manual reconciliation."),

        new(
            Name: "TransactionLifecycleCompensationSpike",
            MetricName: "whyce.economic.transaction.lifecycle.compensated",
            Threshold: 3,
            WindowSeconds: 300,
            Severity: AlertSeverity.Critical,
            Summary: "Multiple transaction lifecycle compensations in 5 minutes — systematic failure",
            Runbook: "1. Check for infrastructure outage (Postgres, Kafka). " +
                     "2. Review compensated transactions for settlement finality mismatch. " +
                     "3. Cross-reference whyce.economic.settlement.reconciliation_required. " +
                     "4. If settlement reconciliation required: coordinate with payment provider."),

        new(
            Name: "SettlementReconciliationRequired",
            MetricName: "whyce.economic.settlement.reconciliation_required",
            Threshold: 1,
            WindowSeconds: 60,
            Severity: AlertSeverity.Critical,
            Summary: "Settlement compensation after external initiation — internal/external state mismatch",
            Runbook: "1. IMMEDIATELY check provider dashboard for settlement status. " +
                     "2. If provider confirmed: initiate manual reversal with provider. " +
                     "3. If provider pending: wait for provider timeout, then verify. " +
                     "4. Update reconciliation process with resolution."),

        new(
            Name: "LockStateUnavailable",
            MetricName: "whyce.enforcement.lock.unavailable_blocks",
            Threshold: 1,
            WindowSeconds: 60,
            Severity: AlertSeverity.Critical,
            Summary: "Enforcement lock state unavailable — commands being fail-closed blocked",
            Runbook: "1. Check Postgres projection_economic_enforcement_lock connectivity. " +
                     "2. Check projection consumer lag for lock topic. " +
                     "3. If Postgres is down: prioritize restore — all commands for affected subjects are blocked. " +
                     "4. Monitor whyce.enforcement.cache.hits for cache fallback effectiveness."),

        // ── Operational health alerts (Warning — ticket within 4h) ────

        new(
            Name: "SettlementFailureRate",
            MetricName: "whyce.economic.settlement.failure",
            Threshold: 5,
            WindowSeconds: 300,
            Severity: AlertSeverity.Warning,
            Summary: "Elevated settlement initiation failures",
            Runbook: "1. Check settlement provider connectivity. " +
                     "2. Review settlement failure reasons in structured logs. " +
                     "3. If provider outage: monitor compensated transaction count."),

        new(
            Name: "RecoveryQueueEscalations",
            MetricName: "whyce.economic.recovery.queue.escalations",
            Threshold: 3,
            WindowSeconds: 300,
            Severity: AlertSeverity.Warning,
            Summary: "Workflow steps escalating to recovery queue — transient failures not resolving",
            Runbook: "1. Check recovery queue depth. " +
                     "2. Verify recovery worker is running. " +
                     "3. Check underlying infrastructure (Postgres, Kafka) for intermittent failures."),

        new(
            Name: "EnforcementActionRate",
            MetricName: "whyce.enforcement.actions.count",
            Threshold: 50,
            WindowSeconds: 300,
            Severity: AlertSeverity.Warning,
            Summary: "High enforcement action rate — many subjects being blocked/restricted",
            Runbook: "1. Check if enforcement rules changed recently. " +
                     "2. Review violation detection for false positives. " +
                     "3. Verify OPA rego rules are not overly broad."),

        // ── Observability alerts (Info — dashboard only) ──────────────

        new(
            Name: "EnforcementCacheHitRate",
            MetricName: "whyce.enforcement.cache.hits",
            Threshold: 10,
            WindowSeconds: 60,
            Severity: AlertSeverity.Info,
            Summary: "High enforcement cache hit rate — projections may be lagging",
            Runbook: "Check projection consumer lag. High cache hits indicate enforcement decisions " +
                     "are being resolved from the in-memory cache rather than projections."),
    };
}

public sealed record AlertRule(
    string Name,
    string MetricName,
    double Threshold,
    int WindowSeconds,
    AlertSeverity Severity,
    string Summary,
    string Runbook);

public enum AlertSeverity
{
    Info,
    Warning,
    Critical
}
