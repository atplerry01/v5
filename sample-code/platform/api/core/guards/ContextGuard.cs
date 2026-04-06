using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Api.Core.Contracts.Context;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Guards;

/// <summary>
/// E19.17.4 — Identity and Context Hardening Guard.
/// Validates that EVERY request carries all mandatory context fields.
///
/// REQUIRED:
/// - WhyceId (resolved identity)
/// - CorrelationId (deterministic, from CorrelationMiddleware)
/// - TraceId (derived from CorrelationId)
/// - TenantContext (resolved by TenantRegionMiddleware)
/// - RegionContext (resolved by TenantRegionMiddleware)
///
/// Rejects the request if ANY context field is missing.
/// This guard runs AFTER all enrichment middleware has executed.
/// </summary>
public static class ContextGuard
{
    /// <summary>
    /// Validates all mandatory context fields are present on the request.
    /// Returns null if all context is present, or an error response listing missing fields.
    /// </summary>
    public static ApiResponse? Enforce(ApiRequest request)
    {
        var missing = new List<string>();

        // WhyceId — identity must be resolved
        if (string.IsNullOrWhiteSpace(request.WhyceId))
            missing.Add("WhyceId");

        // CorrelationId — must be assigned by CorrelationMiddleware
        var correlationId = request.Headers.GetValueOrDefault("X-Correlation-Id");
        if (string.IsNullOrWhiteSpace(correlationId))
            missing.Add("CorrelationId");

        // TraceId — must be derived by CorrelationMiddleware
        if (string.IsNullOrWhiteSpace(request.TraceId))
            missing.Add("TraceId");

        // TenantContext — must be resolved by TenantRegionMiddleware
        var tenant = TenantRegionHeaderKeys.ExtractTenant(request.Headers);
        if (tenant is null)
            missing.Add("TenantContext");

        // RegionContext — must be resolved by TenantRegionMiddleware
        var region = TenantRegionHeaderKeys.ExtractRegion(request.Headers);
        if (region is null)
            missing.Add("RegionContext");

        if (missing.Count > 0)
        {
            return ApiResponse.BadRequest(
                $"CONTEXT_GUARD_VIOLATION: Missing required context — [{string.Join(", ", missing)}]. " +
                "All context must be resolved by middleware before processing.",
                request.TraceId);
        }

        return null;
    }
}
