namespace Whycespace.Domain.TrustSystem.Identity.Profile;

/// <summary>Topic: whyce.identity.access-profile.role-added</summary>
public sealed record RoleAddedToProfileEvent(
    Guid ProfileId,
    Guid RoleId,
    string RoleName) : DomainEvent;
