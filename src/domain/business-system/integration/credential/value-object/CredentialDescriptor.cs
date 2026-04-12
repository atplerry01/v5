namespace Whycespace.Domain.BusinessSystem.Integration.Credential;

public readonly record struct CredentialDescriptor
{
    public Guid PartnerReference { get; }
    public string CredentialType { get; }

    public CredentialDescriptor(Guid partnerReference, string credentialType)
    {
        if (partnerReference == Guid.Empty)
            throw new ArgumentException("PartnerReference must not be empty.", nameof(partnerReference));

        if (string.IsNullOrWhiteSpace(credentialType))
            throw new ArgumentException("CredentialType must not be empty.", nameof(credentialType));

        PartnerReference = partnerReference;
        CredentialType = credentialType;
    }
}
