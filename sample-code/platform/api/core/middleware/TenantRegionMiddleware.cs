using System.Text.Json;
using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Contracts.Context;
using Whycespace.Platform.Api.Core.Services;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Middleware;

/// <summary>
/// Resolves and validates TenantContext + RegionContext for every request.
/// Runs AFTER IdentityMiddleware (requires resolved WhyceIdentity).
///
/// Resolution strategy:
///   Tenant: resolved from WhyceIdentity via ITenantResolutionService.
///           If WhyceRequest includes explicit Tenant, validates identity has access.
///   Region: resolved from WhyceRequest.Region or request headers (X-Whyce-Region / X-Whyce-Jurisdiction).
///
/// REJECTS if:
///   - Tenant cannot be resolved (no default fallback)
///   - Region cannot be resolved (no ambiguity allowed)
///   - Identity does not belong to the requested tenant (cross-tenant blocked)
///   - Region is not allowed for the tenant
///
/// MUST NOT embed business rules.
/// MUST NOT mutate state.
/// MUST NOT bypass WhycePolicy.
/// </summary>
public sealed class TenantRegionMiddleware : IApiMiddleware
{
    private readonly ITenantResolutionService _tenantResolution;

    public TenantRegionMiddleware(ITenantResolutionService tenantResolution)
    {
        _tenantResolution = tenantResolution;
    }

    public async Task<ApiResponse> InvokeAsync(ApiRequest request, Func<ApiRequest, Task<ApiResponse>> next)
    {
        // Guard: identity must be resolved (by IdentityMiddleware)
        var identity = IdentityHeaderKeys.Extract(request.Headers);
        if (identity is null)
            return ApiResponse.Unauthorized(request.TraceId);

        // Step 1: Resolve tenant
        var tenant = await ResolveTenantAsync(request, identity);
        if (tenant is null)
            return ApiResponse.Forbidden("TENANT_RESOLUTION_FAILED: Unable to resolve tenant context", request.TraceId);

        // Step 2: Validate tenant access (cross-tenant check)
        var tenantAccessValid = await _tenantResolution.ValidateTenantAccessAsync(identity, tenant.TenantId);
        if (!tenantAccessValid)
            return ApiResponse.Forbidden("CROSS_TENANT_ACCESS_DENIED: Identity not authorized for this tenant", request.TraceId);

        // Step 3: Resolve region
        var region = ResolveRegion(request);
        if (region is null)
            return ApiResponse.BadRequest("REGION_RESOLUTION_FAILED: Unable to resolve region context", request.TraceId);

        // Step 4: Validate region is allowed for tenant
        var regionValid = await _tenantResolution.ValidateRegionForTenantAsync(tenant.TenantId, region.Region);
        if (!regionValid)
            return ApiResponse.Forbidden(
                $"REGION_NOT_ALLOWED: Tenant {tenant.TenantId} is not permitted in region '{region.Region}'",
                request.TraceId);

        // Enrich request headers with tenant + region for downstream propagation
        var enriched = request with
        {
            Headers = TenantRegionHeaderKeys.Enrich(request.Headers, tenant, region)
        };

        return await next(enriched);
    }

    /// <summary>
    /// Resolves tenant from explicit request payload or from identity via service.
    /// Explicit tenant in request takes precedence but MUST be validated for access.
    /// </summary>
    private async Task<TenantContext?> ResolveTenantAsync(ApiRequest request, WhyceIdentity identity)
    {
        // Check if WhyceRequest body carries explicit tenant
        var explicitTenant = ExtractTenantFromBody(request.Body);
        if (explicitTenant is not null)
            return explicitTenant;

        // Check request headers for tenant context
        var headerTenant = TenantRegionHeaderKeys.ExtractTenant(request.Headers);
        if (headerTenant is not null)
            return headerTenant;

        // Fall back to identity-based resolution via service
        return await _tenantResolution.ResolveAsync(identity);
    }

    /// <summary>
    /// Resolves region from WhyceRequest body or request headers.
    /// No default fallback — region must be explicit.
    /// </summary>
    private static RegionContext? ResolveRegion(ApiRequest request)
    {
        // Check if WhyceRequest body carries explicit region
        var explicitRegion = ExtractRegionFromBody(request.Body);
        if (explicitRegion is not null)
            return explicitRegion;

        // Check request headers for region context
        var headerRegion = TenantRegionHeaderKeys.ExtractRegion(request.Headers);
        if (headerRegion is not null)
            return headerRegion;

        // Try to derive from Jurisdiction in WhyceRequest body
        var jurisdiction = ExtractJurisdictionFromBody(request.Body);
        if (jurisdiction is not null)
        {
            var regionCode = DeriveRegionFromJurisdiction(jurisdiction);
            if (regionCode is not null)
            {
                return new RegionContext
                {
                    Region = regionCode,
                    Jurisdiction = jurisdiction
                };
            }
        }

        return null;
    }

    private static TenantContext? ExtractTenantFromBody(object body)
    {
        var whyceRequest = DeserializeWhyceRequest(body);
        return whyceRequest?.Tenant;
    }

    private static RegionContext? ExtractRegionFromBody(object body)
    {
        var whyceRequest = DeserializeWhyceRequest(body);
        return whyceRequest?.Region;
    }

    private static string? ExtractJurisdictionFromBody(object body)
    {
        var whyceRequest = DeserializeWhyceRequest(body);
        return whyceRequest?.Jurisdiction;
    }

    private static WhyceRequest? DeserializeWhyceRequest(object body)
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

    /// <summary>
    /// Derives a region code from a jurisdiction string.
    /// E.g., "UK-LAW" → "UK", "NG-LAW" → "NG".
    /// Returns null if the jurisdiction format is unrecognized.
    /// </summary>
    private static string? DeriveRegionFromJurisdiction(string jurisdiction)
    {
        if (string.IsNullOrWhiteSpace(jurisdiction))
            return null;

        var dashIndex = jurisdiction.IndexOf('-');
        return dashIndex > 0 ? jurisdiction[..dashIndex] : null;
    }
}
