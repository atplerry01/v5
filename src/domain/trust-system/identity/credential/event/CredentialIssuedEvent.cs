namespace Whycespace.Domain.TrustSystem.Identity.Credential;

public sealed record CredentialIssuedEvent(
    CredentialId CredentialId,
    CredentialDescriptor Descriptor);
