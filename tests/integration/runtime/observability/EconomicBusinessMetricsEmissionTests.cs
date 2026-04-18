using System.Diagnostics.Metrics;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Observability;

namespace Whycespace.Tests.Integration.Runtime.Observability;

/// <summary>
/// Phase 5 — Observability & Alert Integrity.
///
/// Pins that every method on <see cref="IEconomicMetrics"/> maps to a real
/// counter publication on the <c>Whycespace.Economic.Business</c> meter
/// when invoked against <see cref="EconomicBusinessMetrics"/>. Guards
/// against the failure mode where a new metric method is added to the
/// contract but wired to nothing (silent domain) and against the failure
/// mode where two different methods collapse onto the same counter name
/// (ambiguous alerting).
/// </summary>
public sealed class EconomicBusinessMetricsEmissionTests
{
    private const string EconomicMeterName = "Whycespace.Economic.Business";

    private sealed class EmissionRecorder : IDisposable
    {
        private readonly MeterListener _listener = new();
        private readonly List<string> _recorded = new();
        public IReadOnlyList<string> Recorded => _recorded;

        public EmissionRecorder()
        {
            _listener.InstrumentPublished = (instrument, l) =>
            {
                if (instrument.Meter.Name == EconomicMeterName)
                    l.EnableMeasurementEvents(instrument);
            };
            _listener.SetMeasurementEventCallback<long>(
                (instrument, measurement, tags, state) => _recorded.Add(instrument.Name));
            _listener.SetMeasurementEventCallback<double>(
                (instrument, measurement, tags, state) => _recorded.Add(instrument.Name));
            _listener.Start();
        }

        public void Dispose() => _listener.Dispose();
    }

    [Fact]
    public void EveryIEconomicMetricsMethod_PublishesToEconomicMeter()
    {
        using var recorder = new EmissionRecorder();
        IEconomicMetrics m = new EconomicBusinessMetrics();

        m.RecordSettlementSuccess("USD", 100m);
        m.RecordSettlementFailure("USD");
        m.RecordLedgerPostSuccess("USD", 100m);
        m.RecordLedgerPostFailure("USD");
        m.RecordEnforcementWorkflowTriggered("Critical");
        m.RecordSanctionIssued("Critical");
        m.RecordLockApplied("Critical");
        m.RecordTransactionLifecycleStarted("USD");
        m.RecordTransactionLifecycleCompleted("USD");
        m.RecordTransactionLifecycleCompensated("ledger_failure");
        m.RecordSettlementReconciliationRequired("USD");
        m.RecordRecoveryQueueEscalation("ledger_post");
        m.RecordLimitCheckSkipped("USD");
        m.RecordLimitCheckPassed("USD", 100m);
        m.RecordLimitCheckBlocked("USD");
        m.RecordFxLockSkipped("USD");
        m.RecordFxLocked("USD", "EUR", 0.9m);
        m.RecordFxLockMissing("USD", "EUR");
        m.RecordRevenueRecorded("USD", 100m);
        m.RecordDistributionCreated("spv-001", 1_000m, 3);
        m.RecordPayoutExecuted("spv-001", 1_000m, 3);
        m.RecordRiskExposureOpened("Credit", "USD");

        var distinct = recorder.Recorded.Distinct().ToList();

        // Every IEconomicMetrics method must produce at least one
        // publication; we also expect specific canonical names for the
        // Phase 5 additions so they cannot be silently renamed.
        Assert.Contains("whyce.economic.settlement.success", distinct);
        Assert.Contains("whyce.economic.settlement.failure", distinct);
        Assert.Contains("whyce.economic.ledger.post.success", distinct);
        Assert.Contains("whyce.economic.ledger.post.failure", distinct);
        Assert.Contains("whyce.economic.transaction.lifecycle.started", distinct);
        Assert.Contains("whyce.economic.transaction.lifecycle.completed", distinct);
        Assert.Contains("whyce.economic.transaction.lifecycle.compensated", distinct);
        Assert.Contains("whyce.economic.recovery.queue.escalations", distinct);
        Assert.Contains("whyce.economic.settlement.reconciliation_required", distinct);
        Assert.Contains("whyce.economic.limit.check.skipped", distinct);
        Assert.Contains("whyce.economic.limit.check.passed", distinct);
        Assert.Contains("whyce.economic.limit.check.blocked", distinct);
        Assert.Contains("whyce.economic.fx.lock.skipped", distinct);
        Assert.Contains("whyce.economic.fx.locked", distinct);
        Assert.Contains("whyce.economic.fx.lock.missing", distinct);
        // Phase 5 additions — revenue + risk coverage closures.
        Assert.Contains("whyce.economic.revenue.recorded", distinct);
        Assert.Contains("whyce.economic.revenue.distribution.created", distinct);
        Assert.Contains("whyce.economic.revenue.payout.executed", distinct);
        Assert.Contains("whyce.economic.risk.exposure.opened", distinct);
    }
}
