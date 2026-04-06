using Whycespace.Platform.Adapters;
using Whycespace.Platform.Api.Core.Contracts.Governance;

namespace Whycespace.Platform.Api.Core.Services.Governance;

/// <summary>
/// Projection-backed governance query service.
/// Delegates all queries to ProjectionAdapter → IProjectionQuerySource.
/// Pure read-only mapping — no policy logic, no vote counting, no state mutation.
/// </summary>
public sealed class GovernanceQueryService : IGovernanceQueryService
{
    private readonly ProjectionAdapter _projections;

    public GovernanceQueryService(ProjectionAdapter projections)
    {
        _projections = projections;
    }

    public async Task<GovernanceDecisionView?> GetDecisionAsync(Guid decisionId, CancellationToken cancellationToken = default)
    {
        var response = await _projections.QueryAsync<GovernanceDecisionView>(
            "governance.decision",
            new Dictionary<string, object> { ["decisionId"] = decisionId },
            cancellationToken: cancellationToken);

        if (response.StatusCode is < 200 or >= 300) return null;
        return response.Data as GovernanceDecisionView;
    }

    public async Task<IReadOnlyList<GovernanceDecisionView>> GetDecisionsByClusterAsync(string cluster, CancellationToken cancellationToken = default)
    {
        var response = await _projections.QueryListAsync<GovernanceDecisionView>(
            "governance.decision.by-cluster",
            new Dictionary<string, object> { ["cluster"] = cluster },
            cancellationToken: cancellationToken);

        if (response.StatusCode is < 200 or >= 300) return [];
        return response.Data as IReadOnlyList<GovernanceDecisionView> ?? [];
    }

    public async Task<GovernanceTimelineView?> GetTimelineAsync(Guid decisionId, CancellationToken cancellationToken = default)
    {
        var response = await _projections.QueryAsync<GovernanceTimelineView>(
            "governance.decision.timeline",
            new Dictionary<string, object> { ["decisionId"] = decisionId },
            cancellationToken: cancellationToken);

        if (response.StatusCode is < 200 or >= 300) return null;
        return response.Data as GovernanceTimelineView;
    }
}
