using Whycespace.Platform.Api.Core.Contracts;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Core.Middleware;

/// <summary>
/// Platform-level access control pre-check.
/// Enforces RBAC, ABAC, TrustScore, and Consent requirements
/// using the WhyceIdentity resolved by IdentityMiddleware.
///
/// IMPORTANT: This is PRE-CHECK ONLY (advisory).
/// Final enforcement happens in WhycePolicy at runtime.
/// This middleware provides early rejection for obviously denied requests.
/// </summary>
public sealed class AccessControlMiddleware : IApiMiddleware
{
    private readonly AccessControlOptions _options;

    public AccessControlMiddleware(AccessControlOptions options)
    {
        _options = options;
    }

    public Task<ApiResponse> InvokeAsync(ApiRequest request, Func<ApiRequest, Task<ApiResponse>> next)
    {
        var identity = IdentityHeaderKeys.Extract(request.Headers);

        if (identity is null)
            return Task.FromResult(ApiResponse.Unauthorized(request.TraceId));

        // RBAC: Require at least one allowed role
        var rbacResult = EnforceRbac(identity);
        if (rbacResult is not null)
            return Task.FromResult(rbacResult.Value.ToApiResponse(request.TraceId));

        // ABAC: Require mandatory attributes
        var abacResult = EnforceAbac(identity);
        if (abacResult is not null)
            return Task.FromResult(abacResult.Value.ToApiResponse(request.TraceId));

        // TrustScore: Enforce minimum threshold
        var trustResult = EnforceTrustScore(identity);
        if (trustResult is not null)
            return Task.FromResult(trustResult.Value.ToApiResponse(request.TraceId));

        // Consent: Require mandatory consents
        var consentResult = EnforceConsent(identity);
        if (consentResult is not null)
            return Task.FromResult(consentResult.Value.ToApiResponse(request.TraceId));

        return next(request);
    }

    private AccessDenial? EnforceRbac(WhyceIdentity identity)
    {
        if (_options.RequiredRoles.Count == 0)
            return null;

        var hasRole = identity.Roles.Any(r =>
            _options.RequiredRoles.Contains(r, StringComparer.OrdinalIgnoreCase));

        return hasRole
            ? null
            : new AccessDenial("ACCESS_DENIED_RBAC", "Insufficient role privileges");
    }

    private AccessDenial? EnforceAbac(WhyceIdentity identity)
    {
        foreach (var (key, expectedValue) in _options.RequiredAttributes)
        {
            if (!identity.Attributes.TryGetValue(key, out var actualValue))
                return new AccessDenial("ACCESS_DENIED_ABAC", $"Missing required attribute: {key}");

            if (!string.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase))
                return new AccessDenial("ACCESS_DENIED_ABAC", $"Attribute mismatch: {key}");
        }

        return null;
    }

    private AccessDenial? EnforceTrustScore(WhyceIdentity identity)
    {
        if (identity.TrustScore < _options.MinimumTrustScore)
            return new AccessDenial("ACCESS_DENIED_TRUST",
                $"Trust score {identity.TrustScore:F4} below minimum {_options.MinimumTrustScore:F4}");

        return null;
    }

    private AccessDenial? EnforceConsent(WhyceIdentity identity)
    {
        foreach (var required in _options.RequiredConsents)
        {
            var hasConsent = identity.Consents.Any(c =>
                string.Equals(c, required, StringComparison.OrdinalIgnoreCase));

            if (!hasConsent)
                return new AccessDenial("ACCESS_DENIED_CONSENT", $"Missing required consent: {required}");
        }

        return null;
    }
}

/// <summary>
/// Configuration for access control pre-checks.
/// Defines the minimum requirements for platform entry.
/// </summary>
public sealed record AccessControlOptions
{
    public IReadOnlyList<string> RequiredRoles { get; init; } = new List<string> { "user" };
    public IReadOnlyDictionary<string, string> RequiredAttributes { get; init; } = new Dictionary<string, string>();
    public decimal MinimumTrustScore { get; init; } = 0.3m;
    public IReadOnlyList<string> RequiredConsents { get; init; } = new List<string> { "platform.access" };

    public static AccessControlOptions Default => new();
}

/// <summary>
/// Represents an access control denial with a code and reason.
/// </summary>
internal readonly record struct AccessDenial(string Code, string Reason)
{
    public ApiResponse ToApiResponse(string? traceId) =>
        ApiResponse.Forbidden($"{Code}: {Reason}", traceId);
}
