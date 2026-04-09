namespace Whyce.Shared.Contracts.Infrastructure.Chain;

/// <summary>
/// Thrown by the chain-store adapter when a chain-store I/O call cannot
/// complete because of transport failure, a command-level exception, or
/// because the adapter's circuit breaker is Open.
///
/// phase1.5-S5.2.3 / TC-3 (CHAIN-STORE-CT-BREAKER-01): this is the typed
/// RETRYABLE REFUSAL path that closes the holder side of T-R-02 — the
/// counterpart to <see cref="ChainAnchorWaitTimeoutException"/> (which
/// covers TC-2's wait side). The two together form the canonical
/// chain-anchor failure family. The shape mirrors
/// <c>PolicyEvaluationUnavailableException</c>: a small typed reason
/// string ("breaker_open" / "transport") plus a retry-after hint sourced
/// from <c>ChainAnchorOptions.BreakerWindowSeconds</c>.
///
/// Caller-driven cancellation (an <see cref="OperationCanceledException"/>
/// caused by the request CT) is NOT wrapped — it propagates as-is so the
/// host pipeline observes shutdown semantics directly.
///
/// Chain semantics are non-allowing by construction: no event reaches
/// the chain or the outbox when this exception is thrown. The caller is
/// expected to retry after the indicated interval.
/// </summary>
public sealed class ChainAnchorUnavailableException : Exception
{
    /// <summary>
    /// Low-cardinality reason tag. One of: "breaker_open", "transport".
    /// Surfaced on the problem-details payload for operator visibility.
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// Suggested retry delay in seconds. Surfaces as the HTTP
    /// <c>Retry-After</c> header. Sourced from
    /// <c>ChainAnchorOptions.BreakerWindowSeconds</c>.
    /// </summary>
    public int RetryAfterSeconds { get; }

    public ChainAnchorUnavailableException(
        string reason,
        int retryAfterSeconds,
        string message,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Reason = reason;
        RetryAfterSeconds = retryAfterSeconds;
    }
}
