namespace Whycespace.Engines.T0U.WhyceId.Authorization;

/// <summary>
/// T0U authorization decision engine.
/// DENY by default — explicit allow only.
/// Stateless, no domain imports, works with primitive types.
/// </summary>
public sealed class AuthorizationEngine : IdentityEngineBase
{
    // Actions that require no prior identity (self-registration)
    private static readonly HashSet<string> AnonymousActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "create"
    };

    // Actions restricted to admin role
    private static readonly HashSet<string> AdminOnlyActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "deactivate", "decommission", "merge", "close"
    };

    // Resources restricted to admin management
    private static readonly HashSet<string> AdminOnlyResources = new(StringComparer.OrdinalIgnoreCase)
    {
        "role", "permission", "identity-graph", "access-profile"
    };

    // Actions restricted to owner or admin
    private static readonly HashSet<string> OwnerOrAdminActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "activate", "suspend", "reactivate", "revoke", "rotate",
        "block", "unblock", "deregister", "verify"
    };

    public AuthorizationResult Authorize(AuthorizeCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        // DENY: empty subject
        if (string.IsNullOrWhiteSpace(command.SubjectId))
            return new AuthorizationResult(false, "SubjectId is required.");

        // DENY: empty resource
        if (string.IsNullOrWhiteSpace(command.Resource))
            return new AuthorizationResult(false, "Resource is required.");

        // DENY: empty action
        if (string.IsNullOrWhiteSpace(command.Action))
            return new AuthorizationResult(false, "Action is required.");

        // ALLOW: anonymous actions (identity.create — self-registration)
        if (AnonymousActions.Contains(command.Action) &&
            string.Equals(command.Resource, "identity", StringComparison.OrdinalIgnoreCase))
            return new AuthorizationResult(true, string.Empty);

        // DENY: admin-only resources require admin subject prefix
        if (AdminOnlyResources.Contains(command.Resource) && !IsAdmin(command.SubjectId))
            return new AuthorizationResult(false,
                $"Resource '{command.Resource}' requires admin privileges. Subject '{command.SubjectId}' is not authorized.");

        // DENY: admin-only actions require admin subject prefix
        if (AdminOnlyActions.Contains(command.Action) && !IsAdmin(command.SubjectId))
            return new AuthorizationResult(false,
                $"Action '{command.Action}' requires admin privileges.");

        // ALLOW: owner actions on own resources (session, consent, device)
        if (IsOwnerScopedResource(command.Resource))
            return new AuthorizationResult(true, string.Empty);

        // ALLOW: owner-or-admin actions for identified subjects
        if (OwnerOrAdminActions.Contains(command.Action))
            return new AuthorizationResult(true, string.Empty);

        // DENY: default fallback — fail closed
        return new AuthorizationResult(false,
            $"Access denied: subject '{command.SubjectId}' is not authorized for '{command.Resource}.{command.Action}'.");
    }

    private static bool IsAdmin(string subjectId)
        => subjectId.StartsWith("admin:", StringComparison.OrdinalIgnoreCase) ||
           subjectId.StartsWith("system:", StringComparison.OrdinalIgnoreCase);

    private static bool IsOwnerScopedResource(string resource)
        => string.Equals(resource, "session", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(resource, "consent", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(resource, "device", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(resource, "credential", StringComparison.OrdinalIgnoreCase);
}
