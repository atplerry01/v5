namespace Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

/// <summary>Topic: whyce.identity.service-identity.reactivated</summary>
public sealed record ServiceIdentityReactivatedEvent(
    Guid ServiceIdentityId) : DomainEvent;
