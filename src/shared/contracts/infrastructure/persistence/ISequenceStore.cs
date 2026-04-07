namespace Whyce.Shared.Contracts.Infrastructure.Persistence;

/// <summary>
/// HSID v2.1 sequence store. Persists the next monotonic counter per scope
/// (scope = "{topology}:{seed}"). Backs <c>PersistedSequenceResolver</c>;
/// MUST NOT be used for event-sourcing or domain state.
///
/// Guard reference: deterministic-id.guard.md G16.
/// </summary>
public interface ISequenceStore
{
    /// <summary>
    /// Atomically increment the counter for <paramref name="scope"/> and
    /// return the post-increment value (1-based).
    /// </summary>
    Task<long> NextAsync(string scope);

    /// <summary>
    /// Verify the underlying store is reachable AND the
    /// <c>hsid_sequences</c> table exists with the expected shape. Called
    /// once at host bootstrap by <c>HsidInfrastructureValidator</c>; runtime
    /// MUST NOT start if this returns false.
    ///
    /// Guard reference: deterministic-id.guard.md G19, G20.
    /// </summary>
    Task<bool> HealthCheckAsync();
}
