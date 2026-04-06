using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Services.Audit;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Controllers;

/// <summary>
/// WhycePlus audit and trace interface controller.
/// READ-ONLY access to pre-built execution trace projections.
///
/// Provides end-to-end traceability:
///   Command → Workflow → Steps → Policy → Chain
///
/// PLATFORM GUARDS:
/// - GET only — no POST/PUT/PATCH/DELETE
/// - Identity required (WhyceId)
/// - No event store access, no trace reconstruction
/// - No raw event payloads, no internal commands, no sensitive policy rules
/// - All data from pre-built trace projections
///
/// Endpoints:
///   GET /api/audit/correlation/{correlationId}
///   GET /api/audit/workflow/{workflowId}
///   GET /api/audit/identity/{identityId}
/// </summary>
public sealed class AuditController
{
    private readonly ITraceQueryService _traceService;

    public AuditController(ITraceQueryService traceService)
    {
        _traceService = traceService;
    }

    public async Task<ApiResponse> HandleAsync(
        ApiRequest request,
        CancellationToken cancellationToken = default)
    {
        // Guard: Read-only enforcement
        if (!string.Equals(request.Method, "GET", StringComparison.OrdinalIgnoreCase))
            return ApiResponse.Forbidden(
                "Audit interface is read-only — only GET requests are permitted", request.TraceId);

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
        var segments = endpoint.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Expected: ["api", "audit", {type}, {id}]
        if (segments.Length < 4)
            return ApiResponse.BadRequest("Invalid audit endpoint — expected /api/audit/{type}/{id}", traceId);

        var queryType = segments[2].ToLowerInvariant();
        var queryValue = segments[3];

        return queryType switch
        {
            "correlation" => await HandleCorrelationQuery(queryValue, correlationId, traceId, cancellationToken),
            "workflow" => await HandleWorkflowQuery(queryValue, correlationId, traceId, cancellationToken),
            "identity" => await HandleIdentityQuery(queryValue, correlationId, traceId, cancellationToken),
            _ => ApiResponse.NotFound($"Unknown audit query type: {queryType}", traceId)
        };
    }

    private async Task<ApiResponse> HandleCorrelationQuery(
        string queryCorrelationId, string correlationId, string? traceId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(queryCorrelationId))
            return ApiResponse.BadRequest("CorrelationId is required", traceId);

        var trace = await _traceService.GetByCorrelationIdAsync(queryCorrelationId, ct);
        return trace is not null
            ? ApiResponse.Ok(WhyceResponse.Ok(trace, correlationId, traceId), traceId)
            : ApiResponse.NotFound("Trace not found", traceId);
    }

    private async Task<ApiResponse> HandleWorkflowQuery(
        string workflowIdStr, string correlationId, string? traceId, CancellationToken ct)
    {
        if (!Guid.TryParse(workflowIdStr, out var workflowId))
            return ApiResponse.BadRequest("Invalid workflow ID format", traceId);

        var trace = await _traceService.GetByWorkflowIdAsync(workflowId, ct);
        return trace is not null
            ? ApiResponse.Ok(WhyceResponse.Ok(trace, correlationId, traceId), traceId)
            : ApiResponse.NotFound("Trace not found", traceId);
    }

    private async Task<ApiResponse> HandleIdentityQuery(
        string identityIdStr, string correlationId, string? traceId, CancellationToken ct)
    {
        if (!Guid.TryParse(identityIdStr, out var identityId))
            return ApiResponse.BadRequest("Invalid identity ID format", traceId);

        var traces = await _traceService.GetByIdentityAsync(identityId, ct);
        return ApiResponse.Ok(
            WhyceResponse.Ok(traces, correlationId, traceId), traceId);
    }
}
