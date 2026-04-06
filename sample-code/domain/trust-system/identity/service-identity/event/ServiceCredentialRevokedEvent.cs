namespace Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

/// <summary>Topic: whyce.identity.service-identity.credential-revoked</summary>
public sealed record ServiceCredentialRevokedEvent(
    Guid ServiceIdentityId,
    Guid CredentialId) : DomainEvent;
