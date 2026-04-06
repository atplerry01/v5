namespace Whycespace.Domain.TrustSystem.Identity.Trust;

/// <summary>
/// Topic: whyce.identity.trust.initialized
/// </summary>
public sealed record TrustProfileInitializedEvent(
    Guid TrustProfileId,
    Guid IdentityId) : DomainEvent;
