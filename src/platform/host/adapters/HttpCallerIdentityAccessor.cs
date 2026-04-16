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
        // D11: background workers explicitly opt into a system identity scope
        // before dispatching. The scope is checked FIRST so HTTP-context fall-
        // through stays canonical for real HTTP requests (WP-1 deny-by-default
        // preserved). Scope is AsyncLocal-bound, so it cannot leak across
        // unrelated requests.
        if (SystemIdentityScope.Current is { } scope)
            return scope.ActorId;

        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException(
                "WP-1 FAIL-CLOSED: No HTTP context available. " +
                "ICallerIdentityAccessor must be called within an HTTP request scope " +
                "or wrapped in a SystemIdentityScope (background workers).");

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

    public string[] GetRoles()
    {
        if (SystemIdentityScope.Current is { } scope)
            return scope.Roles.ToArray();

        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException(
                "WP-1 FAIL-CLOSED: No HTTP context available.");

        var principal = httpContext.User;
        if (principal?.Identity?.IsAuthenticated != true)
            throw new InvalidOperationException(
                "WP-1 FAIL-CLOSED: No authenticated identity on current request.");

        // Pull from every conventional source the project may emit:
        //   ClaimTypes.Role  — System.Security.Claims default
        //   "role"           — compact JWT spec
        //   "roles"          — the RoleClaimType configured in
        //                      AuthenticationInfrastructureModule
        // Each may produce zero, one, or many claims; concatenate them all
        // and let the normalisation pass deduplicate.
        var raw = principal.FindAll(ClaimTypes.Role).Select(c => c.Value)
            .Concat(principal.FindAll("role").Select(c => c.Value))
            .Concat(principal.FindAll("roles").Select(c => c.Value));

        return raw
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim().ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    // Canonical rego attribute keys. Typed so the evaluator can forward each
    // claim to OPA with the shape rego expects (bool/number/string), matching
    // the `input.subject.<key>` references in `infrastructure/policy/**/*.rego`.
    private static readonly (string Key, AttributeKind Kind)[] KnownAttributes =
    {
        ("kyc_attestation_present",    AttributeKind.Bool),
        ("kyc_passed",                 AttributeKind.Bool),
        ("attested_external_rail",     AttributeKind.Bool),
        ("compliance_override",        AttributeKind.Bool),
        ("dual_consent_present",       AttributeKind.Bool),
        ("admin_cosign_present",       AttributeKind.Bool),
        ("valuation_source_attested",  AttributeKind.Bool),
        ("is_owner_of_resource",       AttributeKind.Bool),
        ("is_owner_of_account",        AttributeKind.Bool),
        ("is_owner_of_source_account", AttributeKind.Bool),
        ("trust_score",                AttributeKind.Number),
        ("new_owner_trust_score",      AttributeKind.Number),
    };

    private enum AttributeKind { Bool, Number }

    public IReadOnlyDictionary<string, object> GetSubjectAttributes()
    {
        // D11: background workers carry no claim-bearing JWT — return an
        // empty attribute set so OPA evaluations against `== true` / `>= floor`
        // checks still deny-by-default. The system identity itself is the
        // authorization signal (callers must scope their rego accordingly).
        if (SystemIdentityScope.Current is not null)
            return new Dictionary<string, object>(StringComparer.Ordinal);

        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException(
                "WP-1 FAIL-CLOSED: No HTTP context available.");

        var principal = httpContext.User;
        if (principal?.Identity?.IsAuthenticated != true)
            throw new InvalidOperationException(
                "WP-1 FAIL-CLOSED: No authenticated identity on current request.");

        var result = new Dictionary<string, object>(StringComparer.Ordinal);
        foreach (var (key, kind) in KnownAttributes)
        {
            var raw = principal.FindFirstValue(key);
            if (string.IsNullOrWhiteSpace(raw)) continue;

            switch (kind)
            {
                case AttributeKind.Bool:
                    if (bool.TryParse(raw, out var b)) result[key] = b;
                    break;
                case AttributeKind.Number:
                    if (double.TryParse(
                            raw,
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out var n))
                        result[key] = n;
                    break;
            }
        }
        return result;
    }

    public string GetTenantId()
    {
        if (SystemIdentityScope.Current is { } scope)
            return scope.TenantId;

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
