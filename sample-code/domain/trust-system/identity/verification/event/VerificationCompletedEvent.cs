namespace Whycespace.Domain.TrustSystem.Identity.Verification;

/// <summary>
/// Topic: whyce.identity.verification.completed
/// </summary>
public sealed record VerificationCompletedEvent(
    Guid VerificationId,
    Guid IdentityId) : DomainEvent;
