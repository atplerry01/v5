namespace Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

/// <summary>Topic: whyce.identity.service-identity.decommissioned</summary>
public sealed record ServiceIdentityDecommissionedEvent(
    Guid ServiceIdentityId,
    string Reason) : DomainEvent;
