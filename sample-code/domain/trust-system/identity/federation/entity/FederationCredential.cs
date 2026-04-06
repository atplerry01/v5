using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// A credential issued by a federation issuer for a linked identity.
/// </summary>
public sealed class FederationCredential : Entity
{
    public Guid CredentialId { get; private set; }
    public IssuerId IssuerId { get; private set; } = null!;
    public string Type { get; private set; } = null!;
    public DateTimeOffset IssuedAt { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public bool Revoked { get; private set; }

    private FederationCredential() { }

    public static FederationCredential Create(
        IssuerId issuerId,
        string type,
        DateTimeOffset issuedAt,
        DateTimeOffset? expiresAt = null)
    {
        Guard.AgainstNull(issuerId);
        Guard.AgainstEmpty(type);

        return new FederationCredential
        {
            Id = DeterministicIdHelper.FromSeed($"FederationCredential:{issuerId.Value}:{type}:{issuedAt:O}"),
            CredentialId = DeterministicIdHelper.FromSeed($"FederationCredentialId:{issuerId.Value}:{type}:{issuedAt:O}"),
            IssuerId = issuerId,
            Type = type,
            IssuedAt = issuedAt,
            ExpiresAt = expiresAt,
            Revoked = false
        };
    }

    public void Revoke()
    {
        if (Revoked)
            throw new InvalidOperationException("Credential is already revoked.");
        Revoked = true;
    }

    public bool IsExpired(DateTimeOffset now) =>
        ExpiresAt.HasValue && now >= ExpiresAt.Value;
}
