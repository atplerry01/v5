using Whycespace.Platform.Api.Core.Controllers;
using Whycespace.Platform.Api.Core.Middleware;
using Whycespace.Platform.Api.Core.Services;
using Whycespace.Platform.Gateway;
using Whycespace.Platform.Middleware;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Platform.Api.Core;

/// <summary>
/// Composes the WhycePlus intent gateway pipeline.
/// Wires middleware chain + controller into a single ApiGateway instance.
///
/// Pipeline order (E19.14 updated):
///   1. CorrelationMiddleware       — assigns CorrelationId + TraceId
///   2. AuthenticationMiddleware    — validates bearer token via runtime (existing)
///   3. IdentityMiddleware          — resolves full WhyceIdentity via IWhyceIdService (T0U)
///   4. TenantRegionMiddleware      — resolves + validates TenantContext + RegionContext (E19.14)
///   5. AccessControlMiddleware     — RBAC + ABAC + TrustScore + Consent pre-check
///   6. RequestValidationMiddleware — validates WhyceRequest schema (including IntentType, Tenant, Region)
///   7. PolicyPreviewMiddleware     — pass-through (preview runs in controller with full context)
///   8. IntentGatewayController     — classify → route → policy preview (advisory) → start workflow
///
/// TenantRegionMiddleware runs AFTER IdentityMiddleware (requires resolved WhyceIdentity)
/// and BEFORE AccessControlMiddleware (tenant context informs access decisions).
/// </summary>
public sealed class IntentGatewayPipeline
{
    private readonly ApiGateway _gateway;

    public IntentGatewayPipeline(
        IIdGenerator idGenerator,
        IClock clock,
        IRuntimeControlPlane controlPlane,
        IWhyceIdService whyceIdService,
        ITenantResolutionService tenantResolution,
        IPolicyPreviewService policyPreview,
        IIntentClassifierService classifier,
        IIntentRoutingService router,
        IWorkflowStartService workflowStart,
        AccessControlOptions? accessControlOptions = null)
    {
        var controller = new IntentGatewayController(classifier, router, policyPreview, workflowStart);

        var middlewares = new List<IApiMiddleware>
        {
            new CorrelationMiddleware(idGenerator, clock),
            new AuthenticationMiddleware(controlPlane, clock),
            new IdentityMiddleware(whyceIdService),
            new TenantRegionMiddleware(tenantResolution),
            new AccessControlMiddleware(accessControlOptions ?? AccessControlOptions.Default),
            new RequestValidationMiddleware(),
            new PolicyPreviewMiddleware()
        };

        _gateway = new ApiGateway(middlewares, req => controller.HandleAsync(req));
    }

    public Task<ApiResponse> ProcessAsync(ApiRequest request) => _gateway.ProcessAsync(request);
}
