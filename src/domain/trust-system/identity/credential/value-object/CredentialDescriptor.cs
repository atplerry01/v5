namespace Whycespace.Domain.TrustSystem.Identity.Credential;

public readonly record struct CredentialDescriptor
{
    public Guid IdentityReference { get; }
    public string CredentialType { get; }

    public CredentialDescriptor(Guid identityReference, string credentialType)
    {
        if (identityReference == Guid.Empty)
            throw new ArgumentException("IdentityReference must not be empty.", nameof(identityReference));
        if (string.IsNullOrWhiteSpace(credentialType))
            throw new ArgumentException("CredentialType must not be empty.", nameof(credentialType));

        IdentityReference = identityReference;
        CredentialType = credentialType;
    }
}
