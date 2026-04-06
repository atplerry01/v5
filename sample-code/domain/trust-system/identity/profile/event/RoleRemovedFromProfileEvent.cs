namespace Whycespace.Domain.TrustSystem.Identity.Profile;

/// <summary>Topic: whyce.identity.access-profile.role-removed</summary>
public sealed record RoleRemovedFromProfileEvent(
    Guid ProfileId,
    Guid RoleId) : DomainEvent;
