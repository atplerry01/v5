namespace Whycespace.Domain.TrustSystem.Identity.Verification;

/// <summary>
/// Topic: whyce.identity.verification.expired
/// </summary>
public sealed record VerificationExpiredEvent(
    Guid VerificationId,
    Guid IdentityId) : DomainEvent;
