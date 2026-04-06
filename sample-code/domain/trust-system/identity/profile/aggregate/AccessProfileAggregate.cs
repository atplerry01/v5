using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.TrustSystem.Identity.Profile;

public sealed class AccessProfileAggregate : AggregateRoot
{
    public AccessProfileId ProfileId { get; private set; } = null!;
    public Guid IdentityId { get; private set; }
    public AccessLevel AccessLevel { get; private set; } = null!;
    public AccessProfileStatus Status { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    private readonly List<ProfileRole> _roles = [];
    public IReadOnlyList<ProfileRole> Roles => _roles.AsReadOnly();

    private readonly List<ProfilePermission> _permissions = [];
    public IReadOnlyList<ProfilePermission> Permissions => _permissions.AsReadOnly();

    private AccessProfileAggregate() { }

    public static AccessProfileAggregate Create(Guid identityId, AccessLevel accessLevel, DateTimeOffset timestamp)
    {
        Guard.AgainstDefault(identityId);
        Guard.AgainstNull(accessLevel);

        var profile = new AccessProfileAggregate
        {
            ProfileId = AccessProfileId.FromSeed($"AccessProfile:{identityId}:{accessLevel.Value}"),
            IdentityId = identityId,
            AccessLevel = accessLevel,
            Status = AccessProfileStatus.Active,
            CreatedAt = timestamp
        };

        profile.Id = profile.ProfileId.Value;
        profile.RaiseDomainEvent(new AccessProfileCreatedEvent(
            profile.ProfileId.Value, identityId, accessLevel.Value));
        return profile;
    }

    public void AddRole(Guid roleId, string roleName, DateTimeOffset timestamp)
    {
        Guard.AgainstDefault(roleId);
        Guard.AgainstEmpty(roleName);
        EnsureActive();
        EnsureInvariant(
            !_roles.Any(r => r.RoleId == roleId && r.IsActive),
            "ROLE_ALREADY_ADDED",
            $"Role '{roleId}' is already in this profile.");

        _roles.Add(ProfileRole.Create(ProfileId.Value, roleId, roleName, timestamp));
        RaiseDomainEvent(new RoleAddedToProfileEvent(ProfileId.Value, roleId, roleName));
    }

    public void RemoveRole(Guid roleId)
    {
        Guard.AgainstDefault(roleId);
        var role = _roles.FirstOrDefault(r => r.RoleId == roleId && r.IsActive);
        EnsureInvariant(role is not null, "ROLE_NOT_IN_PROFILE", $"Role '{roleId}' not found in profile.");
        role!.Deactivate();
        RaiseDomainEvent(new RoleRemovedFromProfileEvent(ProfileId.Value, roleId));
    }

    public void AddPermission(Guid permissionId, string permissionName, string resource, DateTimeOffset timestamp)
    {
        Guard.AgainstDefault(permissionId);
        Guard.AgainstEmpty(permissionName);
        Guard.AgainstEmpty(resource);
        EnsureActive();
        EnsureInvariant(
            !_permissions.Any(p => p.PermissionId == permissionId && p.IsActive),
            "PERMISSION_ALREADY_ADDED",
            $"Permission '{permissionId}' is already in this profile.");

        _permissions.Add(ProfilePermission.Create(ProfileId.Value, permissionId, permissionName, resource, timestamp));
        RaiseDomainEvent(new PermissionAddedToProfileEvent(ProfileId.Value, permissionId, permissionName));
    }

    public void RemovePermission(Guid permissionId)
    {
        Guard.AgainstDefault(permissionId);
        var permission = _permissions.FirstOrDefault(p => p.PermissionId == permissionId && p.IsActive);
        EnsureInvariant(permission is not null, "PERMISSION_NOT_IN_PROFILE", $"Permission '{permissionId}' not found.");
        permission!.Deactivate();
        RaiseDomainEvent(new PermissionRemovedFromProfileEvent(ProfileId.Value, permissionId));
    }

    public void Suspend()
    {
        EnsureActive();
        Status = AccessProfileStatus.Suspended;
        RaiseDomainEvent(new AccessProfileSuspendedEvent(ProfileId.Value, IdentityId));
    }

    public void Reactivate()
    {
        EnsureInvariant(
            Status == AccessProfileStatus.Suspended,
            "PROFILE_MUST_BE_SUSPENDED",
            "Profile is not suspended.");

        Status = AccessProfileStatus.Active;
        RaiseDomainEvent(new AccessProfileReactivatedEvent(ProfileId.Value, IdentityId));
    }

    private void EnsureActive()
    {
        EnsureInvariant(
            Status == AccessProfileStatus.Active,
            "PROFILE_MUST_BE_ACTIVE",
            "Access profile is not active.");
    }
}
