namespace Whycespace.Domain.TrustSystem.Identity.Verification;

public sealed record VerificationInitiatedEvent(
    VerificationId VerificationId,
    VerificationSubject Subject);
