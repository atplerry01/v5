namespace Whycespace.Domain.TrustSystem.Identity.Trust;

/// <summary>
/// Topic: whyce.identity.trust.unfrozen
/// </summary>
public sealed record TrustProfileUnfrozenEvent(
    Guid TrustProfileId,
    Guid IdentityId) : DomainEvent;
