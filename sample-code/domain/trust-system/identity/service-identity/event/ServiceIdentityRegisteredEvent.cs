namespace Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

/// <summary>Topic: whyce.identity.service-identity.registered</summary>
public sealed record ServiceIdentityRegisteredEvent(
    Guid ServiceIdentityId,
    string ServiceName,
    string ServiceType) : DomainEvent;
