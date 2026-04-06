namespace Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

/// <summary>Topic: whyce.identity.service-identity.suspended</summary>
public sealed record ServiceIdentitySuspendedEvent(
    Guid ServiceIdentityId,
    string Reason) : DomainEvent;
