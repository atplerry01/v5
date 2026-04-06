namespace Whycespace.Domain.TrustSystem.Identity.Profile;

/// <summary>Topic: whyce.identity.access-profile.permission-added</summary>
public sealed record PermissionAddedToProfileEvent(
    Guid ProfileId,
    Guid PermissionId,
    string PermissionName) : DomainEvent;
