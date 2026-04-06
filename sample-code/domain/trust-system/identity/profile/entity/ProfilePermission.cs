using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.TrustSystem.Identity.Profile;

public sealed class ProfilePermission : Entity
{
    public Guid ProfileId { get; private set; }
    public Guid PermissionId { get; private set; }
    public string PermissionName { get; private set; } = string.Empty;
    public string Resource { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset AddedAt { get; private set; }

    private ProfilePermission() { }

    public static ProfilePermission Create(Guid profileId, Guid permissionId, string name, string resource, DateTimeOffset timestamp)
    {
        return new ProfilePermission
        {
            Id = DeterministicIdHelper.FromSeed($"ProfilePermission:{profileId}:{permissionId}"),
            ProfileId = profileId,
            PermissionId = permissionId,
            PermissionName = name,
            Resource = resource,
            IsActive = true,
            AddedAt = timestamp
        };
    }

    public void Deactivate() => IsActive = false;
}
