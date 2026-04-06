namespace Whycespace.Domain.TrustSystem.Identity.Profile;

/// <summary>Topic: whyce.identity.access-profile.suspended</summary>
public sealed record AccessProfileSuspendedEvent(
    Guid ProfileId,
    Guid IdentityId) : DomainEvent;
