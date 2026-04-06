using Whycespace.Shared.Contracts.Systems.Intent;

namespace Whycespace.Systems.Upstream.Federation;

/// <summary>
/// Federation region registry — composition only.
/// Declares known regions and their health status routing.
/// NO execution, NO domain mutation, NO persistence.
/// </summary>
public sealed class RegionRegistryService
{
    private readonly ISystemIntentDispatcher _intentDispatcher;

    private static readonly IReadOnlyList<RegionRegistration> RegisteredRegions =
    [
        new("eu-west", "EU West", "europe", true),
        new("us-east", "US East", "americas", true),
        new("ap-south", "Asia Pacific South", "asia-pacific", true),
        new("af-south", "Africa South", "africa", false),
    ];

    public RegionRegistryService(ISystemIntentDispatcher intentDispatcher)
    {
        _intentDispatcher = intentDispatcher;
    }

    public IReadOnlyList<RegionRegistration> GetRegisteredRegions() => RegisteredRegions;

    public async Task<IntentResult> GetRegionHealthAsync(string regionId, CancellationToken cancellationToken = default)
    {
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.Empty,
            CommandType = "federation.region.health-check",
            Payload = new { RegionId = regionId },
            CorrelationId = string.Empty,
            Timestamp = default,
            Headers = new Dictionary<string, string> { ["x-target-region"] = regionId }
        }, cancellationToken);
    }
}

public sealed record RegionRegistration(
    string RegionId,
    string DisplayName,
    string Continent,
    bool IsActive);
