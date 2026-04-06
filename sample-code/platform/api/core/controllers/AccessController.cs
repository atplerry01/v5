using System.Text.Json;
using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Contracts.Access;
using Whycespace.Platform.Api.Core.Services.Access;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Controllers;

/// <summary>
/// WhycePlus access control dashboard controller.
/// READ-ONLY access to identity roles, attributes, TrustScore, consents.
/// Optional advisory access decision preview via POST /preview.
///
/// PLATFORM GUARDS:
/// - GET: read-only, projection-sourced identity access state
/// - POST: only /api/access/preview (advisory, runtime-delegated)
/// - Identity required (WhyceId)
/// - Self-access by default; admin/governance roles for cross-identity
/// - No role assignment, no permission modification
/// - No raw policy rules, no internal scoring algorithms
///
/// Endpoints:
///   GET  /api/access/{identityId}
///   POST /api/access/preview
/// </summary>
public sealed class AccessController
{
    private readonly IAccessQueryService _queryService;
    private readonly IAccessPreviewService _previewService;

    public AccessController(IAccessQueryService queryService, IAccessPreviewService previewService)
    {
        _queryService = queryService;
        _previewService = previewService;
    }

    public async Task<ApiResponse> HandleAsync(
        ApiRequest request,
        CancellationToken cancellationToken = default)
    {
        // Guard: Identity required
        if (string.IsNullOrWhiteSpace(request.WhyceId))
            return ApiResponse.Unauthorized(request.TraceId);

        var correlationId = request.Headers.GetValueOrDefault("X-Correlation-Id") ?? request.RequestId;
        var traceId = request.TraceId ?? request.RequestId;

        return request.Method.ToUpperInvariant() switch
        {
            "GET" => await HandleQuery(request.Endpoint, correlationId, traceId, cancellationToken),
            "POST" => await HandlePreview(request, correlationId, traceId, cancellationToken),
            _ => ApiResponse.Forbidden(
                "Access interface supports GET (queries) and POST /preview only", traceId)
        };
    }

    private async Task<ApiResponse> HandleQuery(
        string endpoint,
        string correlationId,
        string? traceId,
        CancellationToken cancellationToken)
    {
        var segments = endpoint.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // GET /api/access/{identityId}
        if (segments.Length < 3)
            return ApiResponse.BadRequest("Invalid access endpoint — expected /api/access/{identityId}", traceId);

        if (!Guid.TryParse(segments[2], out var identityId))
            return ApiResponse.BadRequest("Invalid identity ID format", traceId);

        var accessView = await _queryService.GetIdentityAccessAsync(identityId, cancellationToken);
        return accessView is not null
            ? ApiResponse.Ok(WhyceResponse.Ok(accessView, correlationId, traceId), traceId)
            : ApiResponse.NotFound("Identity access data not found", traceId);
    }

    private async Task<ApiResponse> HandlePreview(
        ApiRequest request,
        string correlationId,
        string traceId,
        CancellationToken cancellationToken)
    {
        // Guard: only /api/access/preview accepts POST
        var segments = request.Endpoint.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 3 || !string.Equals(segments[2], "preview", StringComparison.OrdinalIgnoreCase))
            return ApiResponse.Forbidden("POST is only permitted on /api/access/preview", traceId);

        var previewRequest = ExtractPreviewRequest(request.Body);
        if (previewRequest is null)
            return ApiResponse.BadRequest("Invalid request body — expected AccessPreviewRequest", traceId);

        if (string.IsNullOrWhiteSpace(previewRequest.Resource))
            return ApiResponse.BadRequest("Resource is required", traceId);

        if (string.IsNullOrWhiteSpace(previewRequest.Action))
            return ApiResponse.BadRequest("Action is required", traceId);

        // Extract identity from headers (enriched by IdentityMiddleware)
        var identity = Contracts.IdentityHeaderKeys.Extract(request.Headers);
        var identityId = identity?.IdentityId ?? Guid.Empty;

        var preview = await _previewService.PreviewAsync(
            identityId, previewRequest.Resource, previewRequest.Action, correlationId, cancellationToken);

        return ApiResponse.Ok(WhyceResponse.Ok(preview, correlationId, traceId), traceId);
    }

    private static AccessPreviewRequest? ExtractPreviewRequest(object body)
    {
        if (body is AccessPreviewRequest typed)
            return typed;

        if (body is JsonElement json)
        {
            return JsonSerializer.Deserialize<AccessPreviewRequest>(
                json.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        return null;
    }
}
