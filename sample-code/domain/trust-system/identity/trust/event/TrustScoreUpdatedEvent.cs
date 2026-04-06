namespace Whycespace.Domain.TrustSystem.Identity.Trust;

/// <summary>
/// Topic: whyce.identity.trust.score-updated
/// </summary>
public sealed record TrustScoreUpdatedEvent(
    Guid TrustProfileId,
    Guid IdentityId,
    decimal OldScore,
    decimal NewScore,
    string NewLevel) : DomainEvent;
