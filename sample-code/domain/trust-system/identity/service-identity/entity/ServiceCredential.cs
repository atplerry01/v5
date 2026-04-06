using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

public sealed class ServiceCredential : Entity
{
    public Guid ServiceIdentityId { get; private set; }
    public string ApiKeyHash { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset IssuedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    private ServiceCredential() { }

    public static ServiceCredential Issue(Guid serviceIdentityId, DateTimeOffset expiresAt, DateTimeOffset timestamp)
    {
        return new ServiceCredential
        {
            Id = DeterministicIdHelper.FromSeed($"ServiceCredential:{serviceIdentityId}:{expiresAt:O}"),
            ServiceIdentityId = serviceIdentityId,
            ApiKeyHash = Convert.ToBase64String(DeterministicIdHelper.FromSeed($"ServiceCredentialKey:{serviceIdentityId}:{expiresAt:O}").ToByteArray()),
            IsActive = true,
            IssuedAt = timestamp,
            ExpiresAt = expiresAt
        };
    }

    public void Revoke(DateTimeOffset timestamp)
    {
        IsActive = false;
        RevokedAt = timestamp;
    }
}
