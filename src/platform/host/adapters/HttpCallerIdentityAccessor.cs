using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Adapters;

/// <summary>
/// WP-1 (Security Binding Completion): Extracts authenticated caller identity
/// from the current HTTP context's ClaimsPrincipal. Fail-closed — throws if
/// no valid identity is present. Registered as singleton; reads per-request
/// context via IHttpContextAccessor's async-local storage.
///
/// Claim mapping:
///   ActorId  ← ClaimTypes.NameIdentifier ("sub" in JWT)
///   TenantId ← "tenant" claim, then X-Tenant-Id header, then "default"
/// </summary>
internal sealed class HttpCallerIdentityAccessor : ICallerIdentityAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCallerIdentityAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetActorId()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException(
                "WP-1 FAIL-CLOSED: No HTTP context available. " +
                "ICallerIdentityAccessor must be called within an HTTP request scope.");

        var principal = httpContext.User;
        if (principal?.Identity?.IsAuthenticated != true)
            throw new InvalidOperationException(
                "WP-1 FAIL-CLOSED: No authenticated identity on current request. " +
                "Requests must carry a valid JWT Bearer token. No fallback to 'system'.");

        var actorId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(actorId))
            throw new InvalidOperationException(
                "WP-1 FAIL-CLOSED: Authenticated principal has no 'sub' or NameIdentifier claim. " +
                "The JWT token must contain a subject claim for actor identification.");

        return actorId;
    }

    public string GetTenantId()
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException(
                "WP-1 FAIL-CLOSED: No HTTP context available.");

        var principal = httpContext.User;

        // Prefer claim from token
        var tenantClaim = principal?.FindFirstValue("tenant");
        if (!string.IsNullOrWhiteSpace(tenantClaim))
            return tenantClaim;

        // Fall back to header (advisory, compatible with existing intake partitioning)
        if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValues))
        {
            var headerTenant = headerValues.ToString();
            if (!string.IsNullOrWhiteSpace(headerTenant))
                return headerTenant;
        }

        // Default tenant for authenticated requests without explicit tenant binding
        return "default";
    }
}
