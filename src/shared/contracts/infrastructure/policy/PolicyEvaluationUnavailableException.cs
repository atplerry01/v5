namespace Whycespace.Shared.Contracts.Infrastructure.Policy;

/// <summary>
/// Thrown by <see cref="IPolicyEvaluator"/> implementations when external
/// policy evaluation cannot produce a decision within the declared
/// operational envelope — i.e. timeout, transport failure, non-2xx
/// response, or circuit breaker Open.
///
/// phase1.5-S5.2.1 / PC-2: this is the typed failure path that replaces
/// the previous unbounded propagation of <c>HttpRequestException</c> /
/// <c>EnsureSuccessStatusCode</c>. It carries the canonical bounded
/// response class (<c>RetryableRefusal</c>) so the API edge can map it to
/// HTTP 503 + <c>Retry-After</c> via the dedicated
/// <c>PolicyEvaluationUnavailableExceptionHandler</c> seam — mirroring the
/// phase1-gate-api-edge precedent set by
/// <see cref="ConcurrencyConflictException"/>.
///
/// Policy primacy ($8) is preserved by construction: this exception is
/// caught ONLY inside <c>PolicyMiddleware</c> as the first step of the
/// R-POL-OPA-RETRY-01 retry integration (R2.A.OPA, 2026-04-19). It is
/// never converted to an implicit allow and never produces a successful
/// command result. The retry path is:
///
///     throw → PolicyMiddleware.ExecuteAsync catches →
///             classifies as RuntimeFailureCategory.PolicyEvaluationDeferred →
///             IRetryExecutor.ExecuteAsync retries with deterministic backoff →
///       ┌─ succeeds on a later attempt → normal decision flow (allow/deny)
///       └─ exhausts MaxAttempts → re-throw PolicyEvaluationUnavailableException
///                                 with reason="retry_exhausted:&lt;original&gt;"
///                                 and RetryAfterSeconds preserved → API edge
///                                 surfaces HTTP 503 + Retry-After as before.
///
/// Outside the middleware (API edge, background workers with no executor
/// registered), the exception still bubbles to the 503 exception handler
/// — the pre-R2.A behaviour. The retry path is an enrichment, never a
/// semantic weakening.
/// </summary>
public sealed class PolicyEvaluationUnavailableException : Exception
{
    /// <summary>
    /// Why the policy evaluation could not complete: one of
    /// <c>"timeout"</c>, <c>"transport"</c>, <c>"http_status"</c>,
    /// <c>"breaker_open"</c>. Kept as a string (not an enum) so the
    /// shared/contracts assembly does not grow a public enum surface for
    /// a runtime concern.
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// Suggested retry delay in seconds. Surfaces as the HTTP
    /// <c>Retry-After</c> header at the API edge. Derived from the OPA
    /// breaker window so callers back off at least as long as the breaker
    /// stays Open.
    /// </summary>
    public int RetryAfterSeconds { get; }

    public PolicyEvaluationUnavailableException(string reason, int retryAfterSeconds, string message)
        : base(message)
    {
        Reason = reason;
        RetryAfterSeconds = retryAfterSeconds;
    }

    public PolicyEvaluationUnavailableException(string reason, int retryAfterSeconds, string message, Exception innerException)
        : base(message, innerException)
    {
        Reason = reason;
        RetryAfterSeconds = retryAfterSeconds;
    }
}
