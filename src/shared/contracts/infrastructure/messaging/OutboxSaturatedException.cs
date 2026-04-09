namespace Whyce.Shared.Contracts.Infrastructure.Messaging;

/// <summary>
/// Thrown by <c>PostgresOutboxAdapter.EnqueueAsync</c> when the latest
/// sampled outbox depth is at or above
/// <see cref="OutboxOptions.HighWaterMark"/>.
///
/// phase1.5-S5.2.1 / PC-3 (OUTBOX-DEPTH-01): this is the typed RETRYABLE
/// REFUSAL path that closes R-02. It carries the canonical bounded
/// response class so the API edge can map it to HTTP 503 +
/// <c>Retry-After</c> via the dedicated
/// <c>OutboxSaturatedExceptionHandler</c> seam — mirroring the
/// phase1-gate-api-edge precedent set by
/// <c>ConcurrencyConflictException</c> and the phase1.5-S5.2.1 / PC-2
/// precedent set by <c>PolicyEvaluationUnavailableException</c>.
///
/// Event semantics are non-allowing by construction: no command's events
/// reach the outbox when this exception is thrown. Events are *refused*,
/// never *dropped silently*. The caller is expected to retry after the
/// indicated interval.
/// </summary>
public sealed class OutboxSaturatedException : Exception
{
    /// <summary>
    /// Sampled outbox depth that triggered the refusal. Surfaced on the
    /// problem-details payload for operator visibility.
    /// </summary>
    public long ObservedDepth { get; }

    /// <summary>
    /// Configured high-water-mark at the time of the refusal.
    /// </summary>
    public long HighWaterMark { get; }

    /// <summary>
    /// Suggested retry delay in seconds. Surfaces as the HTTP
    /// <c>Retry-After</c> header. Sourced from
    /// <c>OutboxOptions.RetryAfterSeconds</c>.
    /// </summary>
    public int RetryAfterSeconds { get; }

    /// <summary>
    /// phase1.5-S5.2.4 / HC-1 (OUTBOX-SNAPSHOT-FRESHNESS-01):
    /// low-cardinality reason tag distinguishing the two refusal
    /// branches inside the same canonical family. One of:
    /// <list type="bullet">
    ///   <item><c>"high_water_mark"</c> — fresh snapshot reported
    ///         depth &gt;= configured HighWaterMark (the original
    ///         PC-3 path).</item>
    ///   <item><c>"snapshot_stale"</c> — the depth snapshot is older
    ///         than <c>OutboxOptions.SnapshotMaxAgeSeconds</c>
    ///         (e.g. <c>OutboxDepthSampler</c> died); fail-safe
    ///         refusal so a frozen below-watermark observation never
    ///         silently admits traffic. Closes H19.</item>
    /// </list>
    /// Both branches throw the same exception type, status code, and
    /// edge-handler shape — only the reason tag differs. No new
    /// refusal family is introduced.
    /// </summary>
    public string Reason { get; }

    public OutboxSaturatedException(long observedDepth, long highWaterMark, int retryAfterSeconds)
        : this(observedDepth, highWaterMark, retryAfterSeconds, reason: "high_water_mark")
    {
    }

    public OutboxSaturatedException(long observedDepth, long highWaterMark, int retryAfterSeconds, string reason)
        : base(BuildMessage(observedDepth, highWaterMark, reason))
    {
        ObservedDepth = observedDepth;
        HighWaterMark = highWaterMark;
        RetryAfterSeconds = retryAfterSeconds;
        Reason = reason;
    }

    private static string BuildMessage(long observedDepth, long highWaterMark, string reason) =>
        reason == "snapshot_stale"
            ? $"Outbox depth snapshot is stale (last observation older than declared SnapshotMaxAgeSeconds). " +
              "Refusing new work fail-safe. No bypass allowed."
            : $"Outbox saturated: depth {observedDepth} ≥ HighWaterMark {highWaterMark}. " +
              "Refusing new work. No bypass allowed.";
}
