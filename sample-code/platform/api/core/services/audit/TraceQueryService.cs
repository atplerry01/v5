using Whycespace.Platform.Adapters;
using Whycespace.Platform.Api.Core.Contracts.Audit;

namespace Whycespace.Platform.Api.Core.Services.Audit;

/// <summary>
/// Projection-backed trace query service.
/// Reads pre-built end-to-end trace projections — no event store access,
/// no reconstruction, no event replay.
/// Single projection call per query — no N+1.
/// </summary>
public sealed class TraceQueryService : ITraceQueryService
{
    private readonly ProjectionAdapter _projections;

    public TraceQueryService(ProjectionAdapter projections)
    {
        _projections = projections;
    }

    public async Task<TraceView?> GetByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        var response = await _projections.QueryAsync<TraceView>(
            "audit.trace.by-correlation",
            new Dictionary<string, object> { ["correlationId"] = correlationId },
            cancellationToken: cancellationToken);

        if (response.StatusCode is < 200 or >= 300) return null;
        return response.Data as TraceView;
    }

    public async Task<TraceView?> GetByWorkflowIdAsync(Guid workflowId, CancellationToken cancellationToken = default)
    {
        var response = await _projections.QueryAsync<TraceView>(
            "audit.trace.by-workflow",
            new Dictionary<string, object> { ["workflowId"] = workflowId },
            cancellationToken: cancellationToken);

        if (response.StatusCode is < 200 or >= 300) return null;
        return response.Data as TraceView;
    }

    public async Task<IReadOnlyList<TraceView>> GetByIdentityAsync(Guid identityId, CancellationToken cancellationToken = default)
    {
        var response = await _projections.QueryListAsync<TraceView>(
            "audit.trace.by-identity",
            new Dictionary<string, object> { ["identityId"] = identityId },
            cancellationToken: cancellationToken);

        if (response.StatusCode is < 200 or >= 300) return [];
        return response.Data as IReadOnlyList<TraceView> ?? [];
    }
}
