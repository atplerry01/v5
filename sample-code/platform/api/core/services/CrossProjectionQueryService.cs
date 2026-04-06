using Whycespace.Shared.Contracts.Infrastructure;

namespace Whycespace.Platform.Api.Core.Services;

/// <summary>
/// Cross-projection query service — enables federated queries across multiple read models.
/// Joins data from different bounded context projections for composite views.
///
/// PLATFORM GUARDS:
/// - READ-ONLY — no mutations
/// - All data sourced from projection store
/// - No direct domain aggregate or event store access
/// </summary>
public sealed class CrossProjectionQueryService
{
    private readonly IProjectionStore _store;

    public CrossProjectionQueryService(IProjectionStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public async Task<ClusterEconomicView?> GetClusterEconomicViewAsync(
        string clusterId,
        CancellationToken cancellationToken = default)
    {
        var clusterProjection = await _store.GetAsync<dynamic>("cluster.state", clusterId, cancellationToken);
        var spvDashboard = await _store.GetAsync<dynamic>("spv.health.dashboard", "spv.health.dashboard", cancellationToken);
        var executionMetrics = await _store.GetAsync<dynamic>("execution.metrics.global", "execution.metrics.global", cancellationToken);

        if (clusterProjection is null)
            return null;

        return new ClusterEconomicView
        {
            ClusterId = clusterId,
            ClusterData = clusterProjection,
            SpvHealthSummary = spvDashboard,
            ExecutionMetrics = executionMetrics
        };
    }
}

public sealed class ClusterEconomicView
{
    public string ClusterId { get; set; } = string.Empty;
    public object? ClusterData { get; set; }
    public object? SpvHealthSummary { get; set; }
    public object? ExecutionMetrics { get; set; }
}
