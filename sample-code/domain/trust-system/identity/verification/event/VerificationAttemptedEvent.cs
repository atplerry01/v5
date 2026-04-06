namespace Whycespace.Domain.TrustSystem.Identity.Verification;

/// <summary>
/// Topic: whyce.identity.verification.attempted
/// </summary>
public sealed record VerificationAttemptedEvent(
    Guid VerificationId,
    Guid IdentityId,
    int AttemptNumber) : DomainEvent;
