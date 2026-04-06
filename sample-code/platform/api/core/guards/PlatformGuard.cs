using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Guards;

/// <summary>
/// Platform layer guard enforcement.
/// Validates critical invariants that must hold for every platform request.
///
/// ENFORCED RULES:
/// - No missing CorrelationId (must be assigned by CorrelationMiddleware)
/// - No missing TraceId (must be assigned by CorrelationMiddleware)
/// - No missing identity (must be resolved by IdentityMiddleware)
/// - No unverified identity
/// - TrustScore above configurable threshold
/// - Required consents present
/// - No direct engine access (platform never references engine types)
/// </summary>
public static class PlatformGuard
{
    public static ApiResponse? EnforceCorrelation(ApiRequest request)
    {
        var correlationId = request.Headers.GetValueOrDefault("X-Correlation-Id");
        if (string.IsNullOrWhiteSpace(correlationId))
            return ApiResponse.ServerError(
                "Platform guard violation: missing CorrelationId — CorrelationMiddleware must run first",
                request.TraceId);

        return null;
    }

    public static ApiResponse? EnforceTraceId(ApiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TraceId))
            return ApiResponse.ServerError(
                "Platform guard violation: missing TraceId — CorrelationMiddleware must run first",
                request.TraceId);

        return null;
    }

    public static ApiResponse? EnforceIdentity(ApiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.WhyceId))
            return ApiResponse.Unauthorized(request.TraceId);

        var identity = IdentityHeaderKeys.Extract(request.Headers);
        if (identity is null)
            return ApiResponse.Unauthorized(request.TraceId);

        if (!identity.IsVerified)
            return ApiResponse.Forbidden(
                "Platform guard violation: identity not verified", request.TraceId);

        return null;
    }

    public static ApiResponse? EnforceTrustScore(ApiRequest request, decimal minimumTrustScore)
    {
        var identity = IdentityHeaderKeys.Extract(request.Headers);
        if (identity is null)
            return ApiResponse.Unauthorized(request.TraceId);

        if (identity.TrustScore < minimumTrustScore)
            return ApiResponse.Forbidden(
                $"Platform guard violation: trust score {identity.TrustScore:F4} below required {minimumTrustScore:F4}",
                request.TraceId);

        return null;
    }

    public static ApiResponse? EnforceConsent(ApiRequest request, IReadOnlyList<string> requiredConsents)
    {
        if (requiredConsents.Count == 0)
            return null;

        var identity = IdentityHeaderKeys.Extract(request.Headers);
        if (identity is null)
            return ApiResponse.Unauthorized(request.TraceId);

        foreach (var required in requiredConsents)
        {
            var hasConsent = identity.Consents.Any(c =>
                string.Equals(c, required, StringComparison.OrdinalIgnoreCase));

            if (!hasConsent)
                return ApiResponse.Forbidden(
                    $"Platform guard violation: missing required consent '{required}'",
                    request.TraceId);
        }

        return null;
    }

    /// <summary>
    /// Runs all platform guards in sequence. Returns the first violation or null if all pass.
    /// </summary>
    public static ApiResponse? EnforceAll(ApiRequest request, decimal minimumTrustScore = 0.3m,
        IReadOnlyList<string>? requiredConsents = null)
    {
        return EnforceCorrelation(request)
            ?? EnforceTraceId(request)
            ?? EnforceIdentity(request)
            ?? EnforceTrustScore(request, minimumTrustScore)
            ?? EnforceConsent(request, requiredConsents ?? new List<string> { "platform.access" });
    }
}
