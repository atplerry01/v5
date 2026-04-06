namespace Whycespace.Domain.TrustSystem.Identity.Profile;

/// <summary>Topic: whyce.identity.access-profile.reactivated</summary>
public sealed record AccessProfileReactivatedEvent(
    Guid ProfileId,
    Guid IdentityId) : DomainEvent;
