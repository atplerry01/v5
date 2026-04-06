using System.Text.Json;
using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Contracts.Governance;
using Whycespace.Platform.Api.Core.Services.Governance;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Controllers;

/// <summary>
/// WhycePlus governance interface controller.
/// READ-ONLY queries for decisions/timelines + CONTROLLED actions routed via WSS.
///
/// PLATFORM GUARDS:
/// - Queries: GET only, projection-sourced
/// - Actions: POST /api/governance/action only, routed through WSS workflows
/// - Identity required (WhyceId)
/// - RBAC: governance roles required
/// - TrustScore: elevated threshold for governance actions
/// - Consent: "governance.access" required
/// - Platform is NEVER the decision authority
/// - No raw policy rules, no internal votes, no sensitive payloads exposed
///
/// Endpoints:
///   GET  /api/governance/{decisionId}
///   GET  /api/governance/{decisionId}/timeline
///   GET  /api/governance/cluster/{cluster}
///   POST /api/governance/action
/// </summary>
public sealed class GovernanceController
{
    private readonly IGovernanceQueryService _queryService;
    private readonly IGovernanceActionService _actionService;

    public GovernanceController(
        IGovernanceQueryService queryService,
        IGovernanceActionService actionService)
    {
        _queryService = queryService;
        _actionService = actionService;
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

        // Route based on HTTP method
        return request.Method.ToUpperInvariant() switch
        {
            "GET" => await HandleQuery(request.Endpoint, correlationId, traceId, cancellationToken),
            "POST" => await HandleAction(request, correlationId, traceId, cancellationToken),
            _ => ApiResponse.Forbidden(
                "Governance interface supports GET (queries) and POST /action (submissions) only", traceId)
        };
    }

    private async Task<ApiResponse> HandleQuery(
        string endpoint,
        string correlationId,
        string? traceId,
        CancellationToken cancellationToken)
    {
        var segments = endpoint.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 3)
            return ApiResponse.BadRequest("Invalid governance endpoint format", traceId);

        var thirdSegment = segments[2].ToLowerInvariant();

        // GET /api/governance/cluster/{cluster}
        if (string.Equals(thirdSegment, "cluster", StringComparison.OrdinalIgnoreCase))
        {
            if (segments.Length < 4)
                return ApiResponse.BadRequest("Cluster name required", traceId);

            var decisions = await _queryService.GetDecisionsByClusterAsync(segments[3], cancellationToken);
            return ApiResponse.Ok(
                WhyceResponse.Ok(decisions, correlationId, traceId), traceId);
        }

        // Parse decisionId
        if (!Guid.TryParse(thirdSegment, out var decisionId))
            return ApiResponse.BadRequest("Invalid decision ID format", traceId);

        // GET /api/governance/{decisionId}/timeline
        if (segments.Length >= 4 && string.Equals(segments[3], "timeline", StringComparison.OrdinalIgnoreCase))
        {
            var timeline = await _queryService.GetTimelineAsync(decisionId, cancellationToken);
            return timeline is not null
                ? ApiResponse.Ok(WhyceResponse.Ok(timeline, correlationId, traceId), traceId)
                : ApiResponse.NotFound("Decision timeline not found", traceId);
        }

        // GET /api/governance/{decisionId}
        var decision = await _queryService.GetDecisionAsync(decisionId, cancellationToken);
        return decision is not null
            ? ApiResponse.Ok(WhyceResponse.Ok(decision, correlationId, traceId), traceId)
            : ApiResponse.NotFound("Decision not found", traceId);
    }

    private async Task<ApiResponse> HandleAction(
        ApiRequest request,
        string correlationId,
        string traceId,
        CancellationToken cancellationToken)
    {
        // Guard: only /api/governance/action endpoint accepts POST
        var segments = request.Endpoint.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 3 || !string.Equals(segments[2], "action", StringComparison.OrdinalIgnoreCase))
            return ApiResponse.Forbidden(
                "POST is only permitted on /api/governance/action", traceId);

        // Parse action request
        var actionRequest = ExtractActionRequest(request.Body);
        if (actionRequest is null)
            return ApiResponse.BadRequest(
                "Invalid request body — expected GovernanceActionRequest", traceId);

        // Validate required fields
        if (actionRequest.DecisionId == Guid.Empty)
            return ApiResponse.BadRequest("DecisionId is required", traceId);

        if (string.IsNullOrWhiteSpace(actionRequest.Action))
            return ApiResponse.BadRequest("Action is required (PROPOSE, APPROVE, REJECT)", traceId);

        // Submit via WSS — platform does NOT decide, only forwards
        var result = await _actionService.SubmitActionAsync(
            actionRequest, request.WhyceId!, correlationId, traceId, cancellationToken);

        if (!result.IsAccepted)
            return ApiResponse.BadRequest(
                $"GOVERNANCE_ACTION_FAILED: {result.ErrorMessage ?? "Submission failed"}", traceId);

        return ApiResponse.Ok(
            WhyceResponse.Accepted(result.WorkflowId, correlationId, traceId), traceId);
    }

    private static GovernanceActionRequest? ExtractActionRequest(object body)
    {
        if (body is GovernanceActionRequest typed)
            return typed;

        if (body is JsonElement json)
        {
            return JsonSerializer.Deserialize<GovernanceActionRequest>(
                json.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        return null;
    }
}
