namespace Whycespace.Domain.TrustSystem.Identity.Verification;

public sealed record VerificationInitiatedEvent(
    VerificationId VerificationId,
    VerificationSubject Subject);

public sealed record VerificationPassedEvent(
    VerificationId VerificationId);

public sealed record VerificationFailedEvent(
    VerificationId VerificationId);
