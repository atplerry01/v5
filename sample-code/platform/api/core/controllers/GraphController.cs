using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Services.Graph;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Controllers;

/// <summary>
/// WhycePlus SPV graph visibility controller.
/// READ-ONLY access to pre-built SPV graph projections.
///
/// PLATFORM GUARDS:
/// - GET only — no POST/PUT/PATCH/DELETE
/// - No mutation, no commands, no state changes
/// - All data sourced from pre-built graph projections via ProjectionAdapter
/// - Identity required (WhyceId must be present)
/// - No domain traversal, no recursive queries, no N+1
/// - Does NOT expose: internal IDs beyond SPV IDs, policy data, raw events, commands
///
/// Endpoints:
///   GET /api/graph/spv/{rootSpvId}
///   GET /api/graph/spv/{spvId}/flows
/// </summary>
public sealed class GraphController
{
    private readonly ISpvGraphQueryService _queryService;

    public GraphController(ISpvGraphQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<ApiResponse> HandleAsync(
        ApiRequest request,
        CancellationToken cancellationToken = default)
    {
        // Guard: Read-only enforcement
        if (!string.Equals(request.Method, "GET", StringComparison.OrdinalIgnoreCase))
            return ApiResponse.Forbidden(
                "Graph interface is read-only — only GET requests are permitted", request.TraceId);

        // Guard: Identity required
        if (string.IsNullOrWhiteSpace(request.WhyceId))
            return ApiResponse.Unauthorized(request.TraceId);

        var correlationId = request.Headers.GetValueOrDefault("X-Correlation-Id") ?? request.RequestId;

        return await RouteQuery(request.Endpoint, correlationId, request.TraceId, cancellationToken);
    }

    private async Task<ApiResponse> RouteQuery(
        string endpoint,
        string correlationId,
        string? traceId,
        CancellationToken cancellationToken)
    {
        // Parse: /api/graph/spv/{id} or /api/graph/spv/{id}/flows
        var segments = endpoint.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Expected: ["api", "graph", "spv", {id}] or ["api", "graph", "spv", {id}, "flows"]
        if (segments.Length < 4 || !string.Equals(segments[2], "spv", StringComparison.OrdinalIgnoreCase))
            return ApiResponse.BadRequest("Invalid graph endpoint — expected /api/graph/spv/{id}", traceId);

        if (!Guid.TryParse(segments[3], out var spvId))
            return ApiResponse.BadRequest("Invalid SPV ID format", traceId);

        // GET /api/graph/spv/{spvId}/flows
        if (segments.Length >= 5 && string.Equals(segments[4], "flows", StringComparison.OrdinalIgnoreCase))
        {
            var flows = await _queryService.GetFlowsAsync(spvId, cancellationToken);
            return ApiResponse.Ok(
                WhyceResponse.Ok(flows, correlationId, traceId), traceId);
        }

        // GET /api/graph/spv/{rootSpvId}
        var graph = await _queryService.GetGraphAsync(spvId, cancellationToken);
        return graph is not null
            ? ApiResponse.Ok(WhyceResponse.Ok(graph, correlationId, traceId), traceId)
            : ApiResponse.NotFound("SPV graph not found", traceId);
    }
}
