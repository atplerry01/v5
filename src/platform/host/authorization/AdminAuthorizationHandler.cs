using Microsoft.AspNetCore.Authorization;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Contracts.Runtime.Admin;

namespace Whycespace.Platform.Host.Authorization;

/// <summary>
/// R4.B / R-ADMIN-SCOPE-01 — authorization requirement for the admin/operator
/// control surface. A request satisfies the requirement iff
/// <see cref="ICallerIdentityAccessor.GetRoles"/> returns a set containing
/// <see cref="AdminScope.RoleName"/> (normalized lower-case). Unauthenticated
/// callers are denied by the underlying <c>[Authorize]</c> attribute; this
/// handler enforces the *admin*-scope promotion on top.
///
/// <para>Why a handler and not <c>[Authorize(Roles = "admin")]</c>: the role
/// matcher uses the canonical accessor (same source of truth the dispatcher
/// sees), so admin scope checks cannot diverge from what the command
/// pipeline observes at evaluation time. A plain <c>[Authorize(Roles =
/// "...")]</c> reads directly from the principal's raw claims and can miss
/// the normalization rules (trim / lower-case / multi-claim merge) the
/// accessor applies.</para>
/// </summary>
public sealed class AdminScopeRequirement : IAuthorizationRequirement { }

public sealed class AdminAuthorizationHandler : AuthorizationHandler<AdminScopeRequirement>
{
    private readonly ICallerIdentityAccessor _callerIdentity;

    public AdminAuthorizationHandler(ICallerIdentityAccessor callerIdentity)
    {
        _callerIdentity = callerIdentity;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdminScopeRequirement requirement)
    {
        // The underlying [Authorize] attribute already guarantees an
        // authenticated principal — without that, HandleRequirementAsync is
        // never invoked. GetRoles() will therefore not throw the fail-closed
        // "no authenticated identity" exception from WP-1.
        string[] roles;
        try
        {
            roles = _callerIdentity.GetRoles();
        }
        catch (InvalidOperationException)
        {
            // No HTTP context / no authenticated principal — deny silently;
            // the framework will emit the configured 401 / 403 challenge.
            return Task.CompletedTask;
        }

        foreach (var role in roles)
        {
            if (string.Equals(role, AdminScope.RoleName, StringComparison.Ordinal))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}
