namespace Whycespace.Domain.TrustSystem.Identity.Verification;

public static class VerificationErrors
{
    public static DomainException NotFound(Guid verificationId)
        => new("VERIFICATION_NOT_FOUND", $"Verification '{verificationId}' was not found.");

    public static DomainException MaxAttemptsReached(Guid verificationId)
        => new("VERIFICATION_MAX_ATTEMPTS", $"Verification '{verificationId}' has reached maximum attempts.");

    public static DomainException AlreadyCompleted(Guid verificationId)
        => new("VERIFICATION_COMPLETED", $"Verification '{verificationId}' is already completed.");
}
