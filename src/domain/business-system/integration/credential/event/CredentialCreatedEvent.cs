namespace Whycespace.Domain.BusinessSystem.Integration.Credential;

public sealed record CredentialRegisteredEvent(CredentialId CredentialId, CredentialDescriptor Descriptor);

public sealed record CredentialActivatedEvent(CredentialId CredentialId);

public sealed record CredentialRevokedEvent(CredentialId CredentialId);
