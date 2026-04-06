namespace Whycespace.Domain.TrustSystem.Identity.Profile;

/// <summary>Topic: whyce.identity.access-profile.permission-removed</summary>
public sealed record PermissionRemovedFromProfileEvent(
    Guid ProfileId,
    Guid PermissionId) : DomainEvent;
