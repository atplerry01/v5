using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.TrustSystem.Identity.Identity;

public sealed class IdentityProfile : Entity
{
    public Guid IdentityId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; private set; }

    private IdentityProfile() { }

    public static IdentityProfile Create(Guid identityId, string key, string value, DateTimeOffset timestamp)
    {
        Guard.AgainstDefault(identityId);
        Guard.AgainstEmpty(key);

        return new IdentityProfile
        {
            Id = DeterministicIdHelper.FromSeed($"IdentityProfile:{identityId}:{key}"),
            IdentityId = identityId,
            Key = key,
            Value = value,
            UpdatedAt = timestamp
        };
    }

    public void Update(string newValue, DateTimeOffset timestamp)
    {
        Value = newValue;
        UpdatedAt = timestamp;
    }
}
