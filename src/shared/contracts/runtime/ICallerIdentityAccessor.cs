namespace Whycespace.Shared.Contracts.Runtime;

/// <summary>
/// Provides the authenticated caller's identity from the current HTTP context.
/// Fail-closed: throws if no authenticated identity is available.
/// Used by SystemIntentDispatcher to populate CommandContext.ActorId and TenantId
/// from the validated JWT token instead of a hardcoded "system" fallback.
///
/// WP-1 (Security Binding Completion): This interface is the boundary seam
/// between HTTP authentication (platform layer) and command context creation
/// (runtime layer). The implementation lives in the host adapter layer and
/// reads from ClaimsPrincipal; the consumer (SystemIntentDispatcher) depends
/// only on this abstraction via shared contracts.
/// </summary>
public interface ICallerIdentityAccessor
{
    /// <summary>
    /// Returns the authenticated caller's actor ID (JWT "sub" claim).
    /// Throws <see cref="InvalidOperationException"/> if no authenticated identity exists.
    /// </summary>
    string GetActorId();

    /// <summary>
    /// Returns the authenticated caller's tenant ID (JWT "tenant" claim or X-Tenant-Id header).
    /// Falls back to "default" only when the claim is absent on an otherwise authenticated request.
    /// </summary>
    string GetTenantId();

    /// <summary>
    /// Returns the authenticated caller's normalized role claims. Roles are
    /// trimmed, lower-cased, deduplicated, and empty values are dropped.
    /// Returns an empty array when the principal carries no role claims;
    /// the caller decides whether to fall back to a default. Throws when no
    /// HTTP context exists or the principal is unauthenticated, matching
    /// <see cref="GetActorId"/> semantics.
    ///
    /// Supports the conventional JWT claim sources used across the project:
    /// <see cref="System.Security.Claims.ClaimTypes.Role"/>, "role", and
    /// "roles" (matching the RoleClaimType configured in the JWT bearer
    /// authentication module).
    /// </summary>
    string[] GetRoles();

    /// <summary>
    /// Returns the authenticated caller's policy-subject attributes, lifted
    /// deterministically from the current principal's JWT claims. Keys mirror
    /// the field names referenced by rego policies (e.g.
    /// <c>kyc_attestation_present</c>, <c>kyc_passed</c>,
    /// <c>attested_external_rail</c>, <c>trust_score</c>). Values are typed
    /// according to the claim shape: boolean claims map to <c>bool</c>,
    /// numeric claims to <c>double</c>, the rest to <c>string</c>.
    ///
    /// Missing claims produce NO entry (rather than a false/zero default),
    /// so OPA evaluation of <c>== true</c> / <c>&gt;= floor</c> against an
    /// absent key remains a deny. This preserves the deny-by-default contract
    /// — a forgotten claim cannot accidentally pass a policy check.
    ///
    /// Returns an empty dictionary when the principal carries none of the
    /// recognised attribute claims. Throws when invoked outside an HTTP
    /// request scope, matching <see cref="GetActorId"/> semantics.
    /// </summary>
    IReadOnlyDictionary<string, object> GetSubjectAttributes();
}
