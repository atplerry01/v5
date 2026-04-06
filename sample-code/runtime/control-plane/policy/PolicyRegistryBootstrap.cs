using Whycespace.Shared.Contracts.Chain;

namespace Whycespace.Runtime.ControlPlane.Policy;

/// <summary>
/// Runtime-owned resilient bootstrap for PolicyRegistryHandler.
/// Retry and backoff logic lives HERE (runtime), not in the engine.
/// Engine remains stateless — runtime handles distribution concerns.
/// </summary>
public sealed class PolicyRegistryBootstrap
{
    private readonly IPolicyRegistryHandler _registry;
    private readonly int _maxRetries;
    private readonly TimeSpan _initialDelay;

    public PolicyRegistryBootstrap(
        IPolicyRegistryHandler registry,
        int maxRetries = 5,
        TimeSpan? initialDelay = null)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _maxRetries = maxRetries;
        _initialDelay = initialDelay ?? TimeSpan.FromSeconds(1);
    }

    public async Task<bool> InitializeWithResilienceAsync(CancellationToken cancellationToken = default)
    {
        var delay = _initialDelay;

        for (var attempt = 1; attempt <= _maxRetries; attempt++)
        {
            try
            {
                await _registry.InitializeAsync(cancellationToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception) when (attempt < _maxRetries)
            {
                await Task.Delay(delay, cancellationToken);
                delay *= 2; // exponential backoff
            }
        }

        // All retries exhausted — check if registry has cached state from prior successful load
        if (_registry.HasCachedState)
            return true; // degrade gracefully with stale cache

        // No cache and DB unavailable — fail fast
        return false;
    }
}
