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

    public OutboxSaturatedException(long observedDepth, long highWaterMark, int retryAfterSeconds)
        : base($"Outbox saturated: depth {observedDepth} ≥ HighWaterMark {highWaterMark}. " +
               "Refusing new work. No bypass allowed.")
    {
        ObservedDepth = observedDepth;
        HighWaterMark = highWaterMark;
        RetryAfterSeconds = retryAfterSeconds;
    }
}
