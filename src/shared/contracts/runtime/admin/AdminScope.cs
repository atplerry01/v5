namespace Whycespace.Shared.Contracts.Runtime.Admin;

/// <summary>
/// R4.B / R-ADMIN-SCOPE-01 — canonical constants for the admin/operator
/// control surface. The policy name is the single source of truth for the
/// admin authorization gate; the role name mirrors the normalized claim
/// surfaced by <c>ICallerIdentityAccessor.GetRoles()</c> (lower-cased,
/// trimmed). No admin endpoint may bypass this gate; no non-admin endpoint
/// may reuse it.
/// </summary>
public static class AdminScope
{
    /// <summary>Canonical authorization policy name wired in AdminAuthorizationModule.</summary>
    public const string PolicyName = "admin-scope";

    /// <summary>Canonical role claim required to satisfy <see cref="PolicyName"/>.</summary>
    public const string RoleName = "admin";

    /// <summary>Canonical route prefix for the admin surface. All admin controllers MUST route under this.</summary>
    public const string RoutePrefix = "api/admin";

    /// <summary>
    /// Audit routing coordinates for operator-action evidence events. Runtime-
    /// owned stream — separate from domain aggregate streams, parallel to the
    /// constitutional policy-decision stream.
    /// </summary>
    public const string AuditClassification = "runtime-system";
    public const string AuditContext = "control-plane";
    public const string AuditDomain = "operator-action";
}
