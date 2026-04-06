using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.TrustSystem.Identity.Identity;

public sealed class IdentityCredential : Entity
{
    public Guid IdentityId { get; private set; }
    public string CredentialType { get; private set; } = string.Empty;
    public DateTimeOffset LinkedAt { get; private set; }

    private IdentityCredential() { }

    public static IdentityCredential Link(Guid identityId, string credentialType, DateTimeOffset timestamp)
    {
        Guard.AgainstDefault(identityId);
        Guard.AgainstEmpty(credentialType);

        return new IdentityCredential
        {
            Id = DeterministicIdHelper.FromSeed($"IdentityCredential:{identityId}:{credentialType}"),
            IdentityId = identityId,
            CredentialType = credentialType,
            LinkedAt = timestamp
        };
    }
}
