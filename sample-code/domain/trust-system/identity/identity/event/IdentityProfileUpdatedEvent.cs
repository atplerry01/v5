namespace Whycespace.Domain.TrustSystem.Identity.Identity;

/// <summary>
/// Topic: whyce.identity.identity.profile-updated
/// </summary>
public sealed record IdentityProfileUpdatedEvent(
    Guid IdentityId,
    string OldDisplayName,
    string NewDisplayName) : DomainEvent;
