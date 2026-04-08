namespace Whyce.Shared.Contracts.Infrastructure.Messaging;

/// <summary>
/// Tunable behavior for the Postgres-to-Kafka outbox relay.
///
/// phase1.6-S1.5 (OUTBOX-CONFIG-01): externalises the previously hardcoded
/// MAX_RETRY constant from KafkaOutboxPublisher. The composition root reads
/// values from configuration (env-var first, per CFG-R1/R2) and constructs
/// this record explicitly — there is no IOptions&lt;T&gt; indirection because
/// no other code in the codebase uses one and adding it would break the
/// "config explicit at the composition root" pattern.
///
/// phase1.5-S5.2.1 / PC-3 (OUTBOX-DEPTH-01): adds a high-water-mark and
/// sampling cadence so the outbox table is no longer an unbounded
/// cross-process buffer between request threads and the publisher loop.
/// HighWaterMark is consulted by PostgresOutboxAdapter.EnqueueAsync via
/// the shared <see cref="IOutboxDepthSnapshot"/> seam (no per-enqueue
/// COUNT(*)). Saturation throws <see cref="OutboxSaturatedException"/>
/// — the canonical RETRYABLE REFUSAL path mapped to HTTP 503 + Retry-After
/// at the API edge.
///
/// Defaults are intentionally conservative and match the pre-S1.5 hardcoded
/// constant where applicable so that omitting the configuration key produces
/// identical runtime behavior.
/// </summary>
public sealed record OutboxOptions
{
    /// <summary>
    /// Maximum number of publish attempts before a row is promoted to
    /// status='deadletter' and (if applicable) republished to the DLQ
    /// topic. Must be at least 1. Default 5 (matches the pre-S1.5
    /// hardcoded constant).
    /// </summary>
    public int MaxRetry { get; init; } = 5;

    /// <summary>
    /// phase1.5-S5.2.1 / PC-3: maximum number of pending+failed outbox
    /// rows tolerated before <c>EnqueueAsync</c> begins refusing new
    /// work with <see cref="OutboxSaturatedException"/>. Read by the
    /// adapter from the shared <see cref="IOutboxDepthSnapshot"/> seam
    /// — never via a per-enqueue COUNT(*). Must be at least 1. Default
    /// 10000 (conservative ceiling sized so a brief broker outage at
    /// modest RPS does not trip immediately, but a sustained outage
    /// converts producer pressure into observable refusals long before
    /// disk pressure).
    /// </summary>
    public int HighWaterMark { get; init; } = 10000;

    /// <summary>
    /// phase1.5-S5.2.1 / PC-3: how often the outbox depth sampler
    /// re-runs its <c>COUNT(*)</c> + <c>MIN(created_at)</c> probe and
    /// publishes the new snapshot to <see cref="IOutboxDepthSnapshot"/>.
    /// Must be at least 1. Default 5 seconds (matches a typical
    /// Prometheus scrape interval and bounds the staleness of the
    /// high-water-mark check at the same order).
    /// </summary>
    public int SamplingIntervalSeconds { get; init; } = 5;

    /// <summary>
    /// phase1.5-S5.2.1 / PC-3: declared response class on saturation.
    /// Locked to <c>RetryableRefusal</c> in Phase 1.5 — §5.2.1 forbids
    /// any shape that could collapse to a silent drop, and the canonical
    /// 4-way model only admits one refusal class for transient outbox
    /// pressure. The field exists so the canonical configuration block
    /// records the choice explicitly per the no-incidental-defaults
    /// rule (R-10).
    /// </summary>
    public string SaturationResponse { get; init; } = "RetryableRefusal";

    /// <summary>
    /// phase1.5-S5.2.1 / PC-3: <c>Retry-After</c> header value, in
    /// seconds, returned with the 503 response. Sized to give the
    /// publisher loop at least one full poll interval to drain. Default
    /// 5 seconds (matches the publisher's 1 s poll cadence × a small
    /// drain margin).
    /// </summary>
    public int RetryAfterSeconds { get; init; } = 5;
}
