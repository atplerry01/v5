using Whycespace.Shared.Contracts.Chain;

namespace Whycespace.Runtime.ControlPlane.Policy;

/// <summary>
/// Runtime consumer for policy.activated events.
/// Invalidates the registry cache on all instances when any policy is activated.
/// Subscribed to Kafka topic: whyce.policy.activated (or Redis pub/sub).
/// RUNTIME ONLY — engine does not know about distribution.
/// </summary>
public sealed class PolicyCacheInvalidationConsumer
{
    private readonly IPolicyRegistryHandler _registry;

    public PolicyCacheInvalidationConsumer(IPolicyRegistryHandler registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public async Task HandleActivationEventAsync(
        Guid policyId,
        CancellationToken cancellationToken = default)
    {
        _registry.InvalidateCache(policyId);
        await _registry.InitializeAsync(cancellationToken);
    }
}
