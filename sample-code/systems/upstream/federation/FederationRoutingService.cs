using Whycespace.Shared.Contracts.Systems.Intent;

namespace Whycespace.Systems.Upstream.Federation;

/// <summary>
/// Federation routing — composition only.
/// Determines which region should handle a command based on
/// jurisdiction, data residency, and latency.
/// NO execution, NO domain mutation, NO persistence.
///
/// Routing decisions are policy-governed: the runtime policy middleware
/// validates jurisdiction compliance before the command reaches the engine.
/// </summary>
public sealed class FederationRoutingService
{
    private readonly ISystemIntentDispatcher _intentDispatcher;

    /// <summary>
    /// Static routing table: jurisdiction → region.
    /// Configured at startup, immutable at runtime.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string> JurisdictionRoutes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["EU"] = "eu-west",
            ["UK"] = "eu-west",
            ["US"] = "us-east",
            ["CA"] = "us-east",
            ["IN"] = "ap-south",
            ["SG"] = "ap-south",
            ["ZA"] = "af-south",
            ["NG"] = "af-south",
        };

    public FederationRoutingService(ISystemIntentDispatcher intentDispatcher)
    {
        _intentDispatcher = intentDispatcher;
    }

    /// <summary>
    /// Resolves the target region for a jurisdiction code.
    /// Returns the default region if no specific mapping exists.
    /// </summary>
    public string ResolveRegion(string jurisdictionCode, string defaultRegion = "eu-west") =>
        JurisdictionRoutes.TryGetValue(jurisdictionCode, out var region) ? region : defaultRegion;

    /// <summary>
    /// Dispatches a command to the resolved region via runtime.
    /// The intent carries the target region as a header — runtime routes accordingly.
    /// </summary>
    public async Task<IntentResult> RouteToRegionAsync(
        ExecuteCommandIntent intent,
        string jurisdictionCode,
        CancellationToken cancellationToken = default)
    {
        var targetRegion = ResolveRegion(jurisdictionCode);

        var federatedIntent = intent with
        {
            Headers = new Dictionary<string, string>(intent.Headers)
            {
                ["x-target-region"] = targetRegion,
                ["x-jurisdiction"] = jurisdictionCode,
                ["x-federated"] = "true"
            }
        };

        return await _intentDispatcher.DispatchAsync(federatedIntent, cancellationToken);
    }
}
