using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.TrustSystem.Identity.Verification;

public sealed class VerificationAttempt : Entity
{
    public Guid VerificationId { get; private set; }
    public string Evidence { get; private set; } = string.Empty;
    public int AttemptNumber { get; private set; }
    public DateTimeOffset AttemptedAt { get; private set; }

    private VerificationAttempt() { }

    public static VerificationAttempt Create(Guid verificationId, string evidence, int attemptNumber, DateTimeOffset timestamp)
    {
        return new VerificationAttempt
        {
            Id = DeterministicIdHelper.FromSeed($"VerificationAttempt:{verificationId}:{attemptNumber}"),
            VerificationId = verificationId,
            Evidence = evidence,
            AttemptNumber = attemptNumber,
            AttemptedAt = timestamp
        };
    }
}
