using System.Text.Json;
using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Contracts.Context;
using Whycespace.Platform.Api.Core.Guards;
using Whycespace.Platform.Api.Core.Mappers;
using Whycespace.Platform.Api.Core.Services;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Controllers;

/// <summary>
/// WhycePlus intent gateway controller.
/// Accepts WhyceRequest → classifies → routes → policy preview → dispatches to WSS.
///
/// PLATFORM GUARDS:
/// - No direct engine or runtime calls
/// - No business logic or domain mutation
/// - All execution flows through: Platform → DownstreamAdapter → WSS → Runtime
/// - Deterministic WorkflowId (no Guid.NewGuid)
/// - ExecutionTarget must be "wss"
/// - PolicyPreview is ADVISORY ONLY — never blocks execution
/// </summary>
public sealed class IntentGatewayController
{
    private readonly IIntentClassifierService _classifier;
    private readonly IIntentRoutingService _router;
    private readonly IPolicyPreviewService _policyPreview;
    private readonly IWorkflowStartService _workflowStart;

    public IntentGatewayController(
        IIntentClassifierService classifier,
        IIntentRoutingService router,
        IPolicyPreviewService policyPreview,
        IWorkflowStartService workflowStart)
    {
        _classifier = classifier;
        _router = router;
        _policyPreview = policyPreview;
        _workflowStart = workflowStart;
    }

    /// <summary>
    /// POST /api/intent handler.
    /// Pipeline: Correlation → Auth → Identity → AccessControl → Validation → (pass-through)
    /// Controller: Classify → Route → PolicyPreview → WSS Dispatch
    /// </summary>
    public async Task<ApiResponse> HandleAsync(ApiRequest request, CancellationToken cancellationToken = default)
    {
        // Platform guard: verify all invariants before processing
        var guardViolation = PlatformGuard.EnforceAll(request);
        if (guardViolation is not null)
            return guardViolation;

        var whyceRequest = ExtractWhyceRequest(request.Body);
        if (whyceRequest is null)
            return ApiResponse.BadRequest("Unable to parse WhyceRequest from body", request.TraceId);

        var correlationId = request.Headers.GetValueOrDefault("X-Correlation-Id") ?? request.RequestId;
        var traceId = request.TraceId ?? request.RequestId;

        // Extract identity from headers (enriched by IdentityMiddleware)
        var identity = IdentityHeaderKeys.Extract(request.Headers);

        // Extract tenant + region from headers (enriched by TenantRegionMiddleware)
        var tenant = TenantRegionHeaderKeys.ExtractTenant(request.Headers);
        var region = TenantRegionHeaderKeys.ExtractRegion(request.Headers);

        // Guard: Tenant and Region must be resolved by TenantRegionMiddleware
        if (tenant is null)
            return ApiResponse.Forbidden("TENANT_REQUIRED: Tenant context not resolved", traceId);
        if (region is null)
            return ApiResponse.BadRequest("REGION_REQUIRED: Region context not resolved", traceId);

        // Step 1: Classify intent via deterministic mapping registry
        var classification = await _classifier.ClassifyAsync(
            whyceRequest, correlationId, traceId, cancellationToken);

        if (!classification.Success || classification.Intent is null)
            return ApiResponse.BadRequest(
                $"INVALID_INTENT: {classification.FailureReason ?? "Classification failed"}", traceId);

        // Enrich classified intent with tenant + region context
        var classified = classification.Intent with
        {
            Tenant = tenant,
            Region = region
        };

        if (string.IsNullOrWhiteSpace(classified.WorkflowKey))
            return ApiResponse.BadRequest("INVALID_INTENT: No workflow key resolved", traceId);

        // Step 2: Resolve route via deterministic routing registry
        var routing = await _router.ResolveRouteAsync(classified, cancellationToken);

        if (!routing.Success || routing.Route is null)
            return ApiResponse.BadRequest(
                $"ROUTING_FAILED: {routing.FailureReason ?? "Route resolution failed"}", traceId);

        // Enrich route with tenant + region context
        var route = routing.Route with
        {
            Tenant = tenant,
            Region = region
        };

        // Guard: ExecutionTarget must be WSS
        if (!string.Equals(route.ExecutionTarget, "wss", StringComparison.OrdinalIgnoreCase))
            return ApiResponse.BadRequest(
                $"ROUTING_FAILED: ExecutionTarget must be 'wss', got '{route.ExecutionTarget}'", traceId);

        // Step 3: Policy preview — ADVISORY ONLY, never blocks
        // Policy input includes tenantId, region, and jurisdiction for jurisdiction-specific/tenant-specific rules
        var preview = await SafePreviewAsync(classified, route, identity, correlationId, cancellationToken);

        // Step 4: Map to WorkflowStartRequest with deterministic WorkflowId
        var startRequest = IntentMapper.ToWorkflowStartRequest(
            whyceRequest, route, correlationId, traceId,
            request.WhyceId ?? whyceRequest.IdentityId, identity, preview,
            tenant, region);

        // Step 5: Dispatch to WSS via adapter (never direct runtime/engine call)
        var result = await _workflowStart.StartAsync(startRequest, cancellationToken);

        if (!result.IsAccepted)
        {
            return ApiResponse.BadRequest(
                $"WORKFLOW_START_FAILED: {result.ErrorMessage ?? "Dispatch failed"}", traceId);
        }

        var whyceResponse = WhyceResponse.Accepted(
            result.WorkflowId,
            correlationId,
            traceId,
            preview);

        return ApiResponse.Ok(whyceResponse, traceId);
    }

    /// <summary>
    /// Safe policy preview — catches all exceptions.
    /// Preview failure must NEVER block execution.
    /// </summary>
    private async Task<PolicyPreview> SafePreviewAsync(
        ClassifiedIntent intent,
        IntentRoute route,
        WhyceIdentity? identity,
        string correlationId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _policyPreview.PreviewAsync(intent, route, identity, correlationId, cancellationToken);
        }
        catch
        {
            return PolicyPreview.ServiceUnavailable();
        }
    }

    private static WhyceRequest? ExtractWhyceRequest(object body)
    {
        if (body is WhyceRequest typed)
            return typed;

        if (body is JsonElement json)
        {
            return JsonSerializer.Deserialize<WhyceRequest>(json.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        return null;
    }
}
