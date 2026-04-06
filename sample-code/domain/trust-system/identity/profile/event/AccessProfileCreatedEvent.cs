namespace Whycespace.Domain.TrustSystem.Identity.Profile;

/// <summary>Topic: whyce.identity.access-profile.created</summary>
public sealed record AccessProfileCreatedEvent(
    Guid ProfileId,
    Guid IdentityId,
    string AccessLevel) : DomainEvent;
