namespace Whycespace.Domain.TrustSystem.Identity.Trust;

/// <summary>
/// Topic: whyce.identity.trust.frozen
/// </summary>
public sealed record TrustProfileFrozenEvent(
    Guid TrustProfileId,
    Guid IdentityId,
    string Reason) : DomainEvent;
