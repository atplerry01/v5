namespace Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

/// <summary>Topic: whyce.identity.service-identity.credential-issued</summary>
public sealed record ServiceCredentialIssuedEvent(
    Guid ServiceIdentityId,
    Guid CredentialId) : DomainEvent;
