namespace Whycespace.Domain.TrustSystem.Identity.Trust;

/// <summary>
/// Topic: whyce.identity.trust.factor-recorded
/// </summary>
public sealed record TrustFactorRecordedEvent(
    Guid TrustProfileId,
    Guid IdentityId,
    string Factor,
    decimal Weight) : DomainEvent;
