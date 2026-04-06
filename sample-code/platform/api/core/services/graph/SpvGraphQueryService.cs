using Whycespace.Platform.Adapters;
using Whycespace.Platform.Api.Core.Contracts.Graph;

namespace Whycespace.Platform.Api.Core.Services.Graph;

/// <summary>
/// Projection-backed SPV graph query service.
/// Reads pre-built graph projections — no domain traversal, no relationship computation.
/// Single projection call returns the complete graph (no N+1).
/// </summary>
public sealed class SpvGraphQueryService : ISpvGraphQueryService
{
    private readonly ProjectionAdapter _projections;

    public SpvGraphQueryService(ProjectionAdapter projections)
    {
        _projections = projections;
    }

    public async Task<SpvGraphView?> GetGraphAsync(Guid rootSpvId, CancellationToken cancellationToken = default)
    {
        var response = await _projections.QueryAsync<SpvGraphView>(
            "graph.spv",
            new Dictionary<string, object> { ["rootSpvId"] = rootSpvId },
            cancellationToken: cancellationToken);

        if (response.StatusCode is < 200 or >= 300)
            return null;

        return response.Data as SpvGraphView;
    }

    public async Task<IReadOnlyList<SpvFlowView>> GetFlowsAsync(Guid spvId, CancellationToken cancellationToken = default)
    {
        var response = await _projections.QueryListAsync<SpvFlowView>(
            "graph.spv.flows",
            new Dictionary<string, object> { ["spvId"] = spvId },
            cancellationToken: cancellationToken);

        if (response.StatusCode is < 200 or >= 300)
            return [];

        return response.Data as IReadOnlyList<SpvFlowView> ?? [];
    }
}
