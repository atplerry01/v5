using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Services.Workflow;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Controllers;

/// <summary>
/// WhycePlus workflow visibility controller.
/// READ-ONLY access to workflow execution projections (CQRS read models).
///
/// PLATFORM GUARDS:
/// - GET only — no POST/PUT/PATCH/DELETE
/// - No mutation, no commands, no state changes
/// - All data sourced from workflow projections via ProjectionAdapter
/// - Identity required (WhyceId must be present)
/// - No direct engine, domain aggregate, or event store access
/// - No event replay or state reconstruction
///
/// Endpoints:
///   GET /api/workflow/{workflowId}
///   GET /api/workflow/{workflowId}/timeline
///   GET /api/workflow/identity/{identityId}
/// </summary>
public sealed class WorkflowController
{
    private readonly IWorkflowQueryService _queryService;

    public WorkflowController(IWorkflowQueryService queryService)
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
                "Workflow interface is read-only — only GET requests are permitted", request.TraceId);

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
        // Parse: /api/workflow/{id} or /api/workflow/{id}/timeline or /api/workflow/identity/{id}
        var segments = endpoint.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length < 3)
            return ApiResponse.BadRequest("Invalid workflow endpoint format", traceId);

        var thirdSegment = segments[2].ToLowerInvariant();

        // GET /api/workflow/identity/{identityId}
        if (string.Equals(thirdSegment, "identity", StringComparison.OrdinalIgnoreCase))
        {
            if (segments.Length < 4 || !Guid.TryParse(segments[3], out var identityId))
                return ApiResponse.BadRequest("Invalid identity ID format", traceId);

            var workflows = await _queryService.GetWorkflowsByIdentityAsync(identityId, cancellationToken);
            return ApiResponse.Ok(
                WhyceResponse.Ok(workflows, correlationId, traceId), traceId);
        }

        // Parse workflowId
        if (!Guid.TryParse(thirdSegment, out var workflowId))
            return ApiResponse.BadRequest("Invalid workflow ID format", traceId);

        // GET /api/workflow/{workflowId}/timeline
        if (segments.Length >= 4 && string.Equals(segments[3], "timeline", StringComparison.OrdinalIgnoreCase))
        {
            var timeline = await _queryService.GetTimelineAsync(workflowId, cancellationToken);
            return timeline is not null
                ? ApiResponse.Ok(WhyceResponse.Ok(timeline, correlationId, traceId), traceId)
                : ApiResponse.NotFound("Workflow timeline not found", traceId);
        }

        // GET /api/workflow/{workflowId}
        var workflow = await _queryService.GetWorkflowAsync(workflowId, cancellationToken);
        return workflow is not null
            ? ApiResponse.Ok(WhyceResponse.Ok(workflow, correlationId, traceId), traceId)
            : ApiResponse.NotFound("Workflow not found", traceId);
    }
}
