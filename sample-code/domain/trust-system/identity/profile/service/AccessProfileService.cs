namespace Whycespace.Domain.TrustSystem.Identity.Profile;

public sealed class AccessProfileService
{
    public bool HasRole(AccessProfileAggregate profile, Guid roleId)
        => profile.Status == AccessProfileStatus.Active
           && profile.Roles.Any(r => r.RoleId == roleId && r.IsActive);

    public bool HasPermission(AccessProfileAggregate profile, Guid permissionId)
        => profile.Status == AccessProfileStatus.Active
           && profile.Permissions.Any(p => p.PermissionId == permissionId && p.IsActive);
}
