namespace Whycespace.Domain.TrustSystem.Identity.Verification;

public sealed class VerificationService
{
    public bool IsExpired(VerificationRecordAggregate record, DateTimeOffset now)
        => record.Status == VerificationStatus.Pending && record.ExpiresAt <= now;

    public bool CanAttempt(VerificationRecordAggregate record)
        => record.Status == VerificationStatus.Pending && record.Attempts.Count < record.MaxAttempts;
}
