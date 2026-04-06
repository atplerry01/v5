using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.TrustSystem.Identity.Profile;

public sealed class ProfileRole : Entity
{
    public Guid ProfileId { get; private set; }
    public Guid RoleId { get; private set; }
    public string RoleName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset AddedAt { get; private set; }

    private ProfileRole() { }

    public static ProfileRole Create(Guid profileId, Guid roleId, string roleName, DateTimeOffset timestamp)
    {
        return new ProfileRole
        {
            Id = DeterministicIdHelper.FromSeed($"ProfileRole:{profileId}:{roleId}"),
            ProfileId = profileId,
            RoleId = roleId,
            RoleName = roleName,
            IsActive = true,
            AddedAt = timestamp
        };
    }

    public void Deactivate() => IsActive = false;
}
