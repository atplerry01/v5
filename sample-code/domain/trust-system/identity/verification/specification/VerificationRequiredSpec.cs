namespace Whycespace.Domain.TrustSystem.Identity.Verification;

public sealed class VerificationRequiredSpec
{
    public bool IsSatisfiedBy(VerificationRecordAggregate entity, DateTimeOffset now)
        => entity.Status == VerificationStatus.Pending
           && entity.ExpiresAt > now
           && entity.Attempts.Count < entity.MaxAttempts;
}
