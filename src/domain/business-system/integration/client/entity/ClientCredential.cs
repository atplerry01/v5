namespace Whycespace.Domain.BusinessSystem.Integration.Client;

public sealed class ClientCredential
{
    public Guid CredentialId { get; }
    public string CredentialType { get; }

    public ClientCredential(Guid credentialId, string credentialType)
    {
        if (credentialId == Guid.Empty)
            throw new ArgumentException("CredentialId must not be empty.", nameof(credentialId));

        if (string.IsNullOrWhiteSpace(credentialType))
            throw new ArgumentException("CredentialType must not be empty.", nameof(credentialType));

        CredentialId = credentialId;
        CredentialType = credentialType;
    }
}
