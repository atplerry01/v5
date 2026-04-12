namespace Whycespace.Domain.TrustSystem.Identity.Credential;

public sealed record CredentialIssuedEvent(
    CredentialId CredentialId,
    CredentialDescriptor Descriptor);

public sealed record CredentialActivatedEvent(
    CredentialId CredentialId);

public sealed record CredentialRevokedEvent(
    CredentialId CredentialId);
