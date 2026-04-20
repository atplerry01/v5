namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// R2.A.D.4 / R-BREAKER-REGISTRY-01 — canonical lookup + enumeration for
/// named <see cref="ICircuitBreaker"/> instances registered in host
/// composition. Replaces the keyed-DI approach from R2.A.D.2 so
/// <see cref="IRuntimeStateAggregator"/> and similar consumers can
/// iterate all registered breakers without knowing their names ahead of
/// time.
///
/// Registry is composed ONCE at startup and immutable thereafter. Names
/// MUST match the breaker's own <see cref="ICircuitBreaker.Name"/> so
/// downstream consumers (metrics, logs, health-posture reasons) use a
/// single string identity.
/// </summary>
public interface ICircuitBreakerRegistry
{
    /// <summary>
    /// Return the breaker registered under <paramref name="name"/>, or
    /// throw <see cref="KeyNotFoundException"/> if none exists. Use when
    /// the caller owns a composition-time invariant that the breaker MUST
    /// exist — fail loud on misconfiguration.
    /// </summary>
    ICircuitBreaker Get(string name);

    /// <summary>
    /// Return the breaker registered under <paramref name="name"/>, or
    /// <c>null</c> if none exists. Use when the caller can operate without
    /// the breaker (feature-flag-gated paths, optional dependencies).
    /// </summary>
    ICircuitBreaker? TryGet(string name);

    /// <summary>
    /// Enumerate every registered breaker. Iteration order is deterministic
    /// (sorted by <see cref="ICircuitBreaker.Name"/>) so health-posture
    /// reason lists are stable across process restarts.
    /// </summary>
    IReadOnlyCollection<ICircuitBreaker> GetAll();
}
