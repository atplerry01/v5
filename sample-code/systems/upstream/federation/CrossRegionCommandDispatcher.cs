using Whycespace.Shared.Contracts.Systems.Intent;

namespace Whycespace.Systems.Upstream.Federation;

/// <summary>
/// Cross-region command dispatcher — composition only.
/// Composes federated command dispatch with region resolution
/// and jurisdiction validation. All execution flows through runtime.
/// NO execution, NO domain mutation, NO persistence.
///
/// Boundary declaration:
/// - BCs touched: federation (identity), policy (constitutional)
/// - Engines composed: none directly — dispatches via runtime
/// - Runtime pipelines: federation routing + policy middleware
/// </summary>
public sealed class CrossRegionCommandDispatcher
{
    private readonly FederationRoutingService _routing;
    private readonly RegionRegistryService _registry;

    public CrossRegionCommandDispatcher(
        FederationRoutingService routing,
        RegionRegistryService registry)
    {
        _routing = routing;
        _registry = registry;
    }

    /// <summary>
    /// Dispatches a command to the appropriate region based on jurisdiction.
    /// Validates that the target region is active before dispatching.
    /// </summary>
    public async Task<IntentResult> DispatchAsync(
        ExecuteCommandIntent intent,
        string jurisdictionCode,
        CancellationToken cancellationToken = default)
    {
        var targetRegion = _routing.ResolveRegion(jurisdictionCode);

        // Verify target region is active via registry
        var regions = _registry.GetRegisteredRegions();
        var region = regions.FirstOrDefault(r => r.RegionId == targetRegion);

        if (region is null || !region.IsActive)
        {
            return IntentResult.Fail(
                intent.CommandId,
                $"Target region '{targetRegion}' for jurisdiction '{jurisdictionCode}' is not available.",
                "FEDERATION_REGION_UNAVAILABLE");
        }

        return await _routing.RouteToRegionAsync(intent, jurisdictionCode, cancellationToken);
    }
}
