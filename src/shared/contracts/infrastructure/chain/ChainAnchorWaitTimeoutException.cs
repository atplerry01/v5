namespace Whyce.Shared.Contracts.Infrastructure.Chain;

/// <summary>
/// Thrown by <c>ChainAnchorService.AnchorAsync</c> when the wait for
/// the global commit serializer exceeds
/// <c>ChainAnchorOptions.WaitTimeoutMs</c>.
///
/// phase1.5-S5.2.3 / TC-2 (CHAIN-ANCHOR-WAIT-TIMEOUT-01): this is the
/// typed RETRYABLE REFUSAL path that closes T-R-02 (indefinite chain
/// anchor wait). It carries the canonical bounded response class so
/// the API edge can map it to HTTP 503 + <c>Retry-After</c> via the
/// dedicated <c>ChainAnchorWaitTimeoutExceptionHandler</c> seam —
/// mirroring the precedents set by
/// <c>PolicyEvaluationUnavailableException</c>,
/// <c>OutboxSaturatedException</c>, and
/// <c>WorkflowSaturatedException</c>.
///
/// Chain semantics are non-allowing by construction: no event reaches
/// the chain or the outbox when this exception is thrown — the
/// critical section is never entered. The caller is expected to retry
/// after the indicated interval.
/// </summary>
public sealed class ChainAnchorWaitTimeoutException : Exception
{
    /// <summary>
    /// Configured wait timeout, in milliseconds, that was exceeded.
    /// Surfaced on the problem-details payload for operator visibility.
    /// </summary>
    public int WaitTimeoutMs { get; }

    /// <summary>
    /// Suggested retry delay in seconds. Surfaces as the HTTP
    /// <c>Retry-After</c> header. Sourced from
    /// <c>ChainAnchorOptions.RetryAfterSeconds</c>.
    /// </summary>
    public int RetryAfterSeconds { get; }

    public ChainAnchorWaitTimeoutException(int waitTimeoutMs, int retryAfterSeconds)
        : base($"ChainAnchor wait exceeded {waitTimeoutMs} ms. " +
               "Refusing new work. No bypass allowed.")
    {
        WaitTimeoutMs = waitTimeoutMs;
        RetryAfterSeconds = retryAfterSeconds;
    }
}
