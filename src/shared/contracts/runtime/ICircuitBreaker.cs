namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// R2.A.D.1 / R-CIRCUIT-BREAKER-01 (D4 LOCKED → per-dependency) —
/// canonical circuit breaker contract. Wraps an operation against a
/// single external dependency and short-circuits calls when the
/// dependency is failing, giving it time to recover.
///
/// State machine:
/// <list type="bullet">
///   <item><b>Closed</b> — operations run normally; each failure increments a counter.</item>
///   <item><b>Closed → Open</b> — counter reaches <c>FailureThreshold</c>. Breaker opens at <c>IClock.UtcNow</c>.</item>
///   <item><b>Open</b> — <see cref="ExecuteAsync{T}"/> throws <see cref="CircuitBreakerOpenException"/> immediately; operation is NOT executed.</item>
///   <item><b>Open → HalfOpen</b> — when <c>IClock.UtcNow - opened_at &gt;= WindowSeconds</c>. The next <see cref="ExecuteAsync{T}"/> call is the trial.</item>
///   <item><b>HalfOpen → Closed</b> — trial succeeds. Failure counter resets.</item>
///   <item><b>HalfOpen → Open</b> — trial throws. <c>opened_at</c> resets; window starts over.</item>
/// </list>
///
/// Failure accounting: every exception from <c>operation</c> counts as a
/// failure EXCEPT <see cref="OperationCanceledException"/> that carries the
/// caller's cancellation token — that is propagation, not evidence of
/// dependency unavailability. Callers wanting typed exception filtering
/// wrap the operation themselves.
///
/// <para>
/// <b>State getter is side-effect-free.</b> Reading <see cref="State"/>
/// does NOT consume the HalfOpen trial slot — health-posture queries can
/// safely poll at any rate. The actual HalfOpen → Closed / Open
/// transition happens inside <see cref="ExecuteAsync{T}"/> on the trial call.
/// </para>
/// </summary>
public interface ICircuitBreaker
{
    /// <summary>Stable identifier for metrics tags, health reports, logs.</summary>
    string Name { get; }

    /// <summary>Side-effect-free snapshot of the current state.</summary>
    CircuitBreakerState State { get; }

    /// <summary>
    /// Run <paramref name="operation"/> under breaker protection. Throws
    /// <see cref="CircuitBreakerOpenException"/> without executing the
    /// operation when the breaker is Open.
    /// </summary>
    Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Circuit breaker state per R-CIRCUIT-BREAKER-01.
/// </summary>
public enum CircuitBreakerState
{
    /// <summary>Operations execute normally; counting consecutive failures.</summary>
    Closed = 0,

    /// <summary>Operations fail-fast via <see cref="CircuitBreakerOpenException"/>. Window is ticking.</summary>
    Open = 1,

    /// <summary>
    /// Window has elapsed; the NEXT ExecuteAsync call becomes the trial.
    /// Health-posture reads see this state but do not consume the trial slot.
    /// </summary>
    HalfOpen = 2
}

/// <summary>
/// Thrown by <see cref="ICircuitBreaker.ExecuteAsync{T}"/> when the breaker
/// is Open. Parallel shape to <see cref="Whycespace.Shared.Contracts.Infrastructure.Policy.PolicyEvaluationUnavailableException"/>
/// so API-edge handlers can map either to HTTP 503 + <c>Retry-After</c>.
/// </summary>
public sealed class CircuitBreakerOpenException : Exception
{
    public string BreakerName { get; }

    /// <summary>
    /// Suggested retry delay in seconds — the remaining breaker window.
    /// Surfaces as HTTP <c>Retry-After</c> at the API edge.
    /// </summary>
    public int RetryAfterSeconds { get; }

    public CircuitBreakerOpenException(string breakerName, int retryAfterSeconds, string message)
        : base(message)
    {
        BreakerName = breakerName;
        RetryAfterSeconds = retryAfterSeconds;
    }

    public CircuitBreakerOpenException(string breakerName, int retryAfterSeconds, string message, Exception innerException)
        : base(message, innerException)
    {
        BreakerName = breakerName;
        RetryAfterSeconds = retryAfterSeconds;
    }
}

/// <summary>
/// Configuration for a single circuit breaker instance per R-CIRCUIT-BREAKER-PER-DEPENDENCY-01.
/// Each external dependency gets its own options sized for its failure profile.
/// </summary>
public sealed record CircuitBreakerOptions
{
    public required string Name { get; init; }

    /// <summary>
    /// Consecutive failures (no interleaved success) that trip the breaker.
    /// Default 5 — balances noise-tolerance against fast outage detection.
    /// </summary>
    public int FailureThreshold { get; init; } = 5;

    /// <summary>
    /// How long the breaker stays Open before admitting a HalfOpen trial.
    /// Default 30s — typical blip-recovery window.
    /// </summary>
    public int WindowSeconds { get; init; } = 30;
}
