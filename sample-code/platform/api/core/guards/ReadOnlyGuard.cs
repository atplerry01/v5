using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Guards;

/// <summary>
/// E19.17.3 — Read-Only Enforcement Guard.
/// Platform is read-only by default. Only explicitly allowed mutation endpoints may accept non-GET requests.
///
/// ALLOWED MUTATIONS:
/// - POST /api/intent          (intent submission → routed to WSS)
/// - POST /api/governance/action (governance action → routed to WSS)
/// - POST /api/simulation      (read-only semantic — no state mutation)
///
/// All other non-GET requests are REJECTED.
/// No PUT, PATCH, DELETE allowed at the platform layer.
/// </summary>
public static class ReadOnlyGuard
{
    private static readonly HashSet<string> AllowedMutationEndpoints = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/intent",
        "/api/governance/action",
        "/api/simulation"
    };

    private static readonly HashSet<string> ReadOnlyMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET",
        "HEAD",
        "OPTIONS"
    };

    /// <summary>
    /// Enforces read-only semantics on the request.
    /// Returns null if the request is allowed, or an error response if blocked.
    /// </summary>
    public static ApiResponse? Enforce(ApiRequest request)
    {
        // Read-only methods always pass
        if (ReadOnlyMethods.Contains(request.Method))
            return null;

        // POST to explicitly allowed endpoints passes
        if (string.Equals(request.Method, "POST", StringComparison.OrdinalIgnoreCase) &&
            IsAllowedMutation(request.Endpoint))
            return null;

        // Everything else is blocked
        return ApiResponse.Forbidden(
            $"READ_ONLY_VIOLATION: Method '{request.Method}' on '{request.Endpoint}' is not permitted. " +
            "Platform enforces read-only semantics — only GET and whitelisted POST endpoints are allowed.",
            request.TraceId);
    }

    private static bool IsAllowedMutation(string endpoint)
    {
        // Normalize: strip trailing slashes, compare case-insensitive
        var normalized = endpoint.TrimEnd('/');

        return AllowedMutationEndpoints.Contains(normalized);
    }
}
