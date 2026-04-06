namespace Whycespace.Shared.Contracts.Chain;

/// <summary>
/// Contract for runtime interaction with the policy registry engine.
/// Provides initialization, cache management, and invalidation.
/// Implementations live in T0U engine — runtime bootstrap calls this via DI.
/// </summary>
public interface IPolicyRegistryHandler
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    bool HasCachedState { get; }
    void InvalidateCache(Guid policyId);
}
