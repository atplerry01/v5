using System.Diagnostics.Metrics;

namespace Whycespace.Runtime.OutboundEffects;

/// <summary>
/// R3.B.1 / R-OUT-EFF-OBS-01 — canonical meter + baseline instruments for the
/// outbound-effect subsystem. Low-cardinality vocabulary bounded by
/// <c>provider × effect_type</c>. New instruments or outcome tags require
/// guard-level promotion (<c>R-OUT-EFF-OBS-02</c>).
/// </summary>
public sealed class OutboundEffectsMeter
{
    public const string MeterName = "Whycespace.OutboundEffects";

    public Meter Meter { get; }
    public Counter<long> Scheduled { get; }
    public Counter<long> ScheduleDedupHit { get; }
    public Histogram<double> DispatchDurationMs { get; }
    public Histogram<double> AckDurationMs { get; }
    public Histogram<double> FinalityDurationMs { get; }
    public Counter<long> RetryAttempts { get; }
    public Counter<long> RetryExhausted { get; }
    public Counter<long> ReconciliationRequired { get; }
    public Counter<long> Cancelled { get; }

    // R3.B.5 — compensation signaling instruments.
    public Counter<long> CompensationEmitted { get; }
    public Counter<long> CompensationUnhandled { get; }
    public Counter<long> CompensationHandlerFailed { get; }

    public OutboundEffectsMeter()
    {
        Meter = new Meter(MeterName);
        Scheduled = Meter.CreateCounter<long>("outbound.effect.scheduled");
        ScheduleDedupHit = Meter.CreateCounter<long>("outbound.effect.schedule.dedup_hit");
        DispatchDurationMs = Meter.CreateHistogram<double>("outbound.effect.dispatch.duration");
        AckDurationMs = Meter.CreateHistogram<double>("outbound.effect.ack.duration");
        FinalityDurationMs = Meter.CreateHistogram<double>("outbound.effect.finality.duration");
        RetryAttempts = Meter.CreateCounter<long>("outbound.effect.retry_attempts");
        RetryExhausted = Meter.CreateCounter<long>("outbound.effect.retry_exhausted");
        ReconciliationRequired = Meter.CreateCounter<long>("outbound.effect.reconciliation_required");
        Cancelled = Meter.CreateCounter<long>("outbound.effect.cancelled");

        CompensationEmitted = Meter.CreateCounter<long>("outbound.effect.compensation.emitted");
        CompensationUnhandled = Meter.CreateCounter<long>("outbound.effect.compensation.unhandled");
        CompensationHandlerFailed = Meter.CreateCounter<long>("outbound.effect.compensation.handler_failed");
    }
}
