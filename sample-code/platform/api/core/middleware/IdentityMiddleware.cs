using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Services;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Middleware;

/// <summary>
/// Enforces identity-first access for the WhycePlus intent gateway.
/// Resolves the full WhyceIdentity via the IWhyceIdService adapter (T0U),
/// validates the session, and enriches the request headers with identity data.
///
/// RULE: NO VALID IDENTITY = NO SYSTEM ACCESS
///
/// Runs AFTER AuthenticationMiddleware (which resolves the WhyceId token).
/// Enriches the request with full identity profile for downstream middleware.
/// </summary>
public sealed class IdentityMiddleware : IApiMiddleware
{
    private readonly IWhyceIdService _whyceIdService;

    public IdentityMiddleware(IWhyceIdService whyceIdService)
    {
        _whyceIdService = whyceIdService;
    }

    public async Task<ApiResponse> InvokeAsync(ApiRequest request, Func<ApiRequest, Task<ApiResponse>> next)
    {
        // Guard: WhyceId must be present (set by AuthenticationMiddleware)
        if (string.IsNullOrWhiteSpace(request.WhyceId))
            return ApiResponse.Unauthorized(request.TraceId);

        // Resolve full identity profile from WhyceID (T0U)
        var identity = await _whyceIdService.ResolveAsync(request.WhyceId);

        if (identity is null)
            return IdentityDeniedResponse("IDENTITY_RESOLUTION_FAILED", request.TraceId);

        if (!identity.IsVerified)
            return IdentityDeniedResponse("IDENTITY_NOT_VERIFIED", request.TraceId);

        // Validate session integrity
        var sessionValid = await _whyceIdService.ValidateSessionAsync(identity.IdentityId);

        if (!sessionValid)
            return IdentityDeniedResponse("SESSION_INVALID", request.TraceId);

        // Enrich request with full identity context for downstream middleware
        var enriched = request with
        {
            Headers = IdentityHeaderKeys.Enrich(request.Headers, identity)
        };

        return await next(enriched);
    }

    private static ApiResponse IdentityDeniedResponse(string reason, string? traceId) =>
        ApiResponse.Unauthorized(traceId);
}
