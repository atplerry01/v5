using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.TrustSystem.Identity.Credential;

public readonly record struct CredentialId(Guid Value)
{
    public static CredentialId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public static readonly CredentialId Empty = new(Guid.Empty);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(CredentialId id) => id.Value;
    public static implicit operator CredentialId(Guid id) => new(id);
}
