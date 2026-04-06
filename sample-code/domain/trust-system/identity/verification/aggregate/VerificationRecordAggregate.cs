using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.TrustSystem.Identity.Verification;

public sealed class VerificationRecordAggregate : AggregateRoot
{
    public VerificationId VerificationId { get; private set; } = null!;
    public Guid IdentityId { get; private set; }
    public VerificationType VerificationType { get; private set; } = null!;
    public VerificationMethod Method { get; private set; } = null!;
    public VerificationStatus Status { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public int MaxAttempts { get; private set; }

    private readonly List<VerificationAttempt> _attempts = [];
    public IReadOnlyList<VerificationAttempt> Attempts => _attempts.AsReadOnly();

    private VerificationRecordAggregate() { }

    public static VerificationRecordAggregate Create(
        Guid identityId,
        VerificationType verificationType,
        VerificationMethod method,
        DateTimeOffset expiresAt,
        DateTimeOffset timestamp,
        int maxAttempts = 3)
    {
        Guard.AgainstDefault(identityId);
        Guard.AgainstNull(verificationType);
        Guard.AgainstNull(method);

        var record = new VerificationRecordAggregate
        {
            VerificationId = VerificationId.FromSeed($"VerificationRecord:{identityId}:{verificationType.Value}:{method.Value}"),
            IdentityId = identityId,
            VerificationType = verificationType,
            Method = method,
            Status = VerificationStatus.Pending,
            CreatedAt = timestamp,
            ExpiresAt = expiresAt,
            MaxAttempts = maxAttempts
        };

        record.Id = record.VerificationId.Value;
        record.RaiseDomainEvent(new VerificationCreatedEvent(
            record.VerificationId.Value, identityId, verificationType.Value, method.Value));
        return record;
    }

    public VerificationAttempt AddAttempt(string evidence, DateTimeOffset timestamp)
    {
        Guard.AgainstEmpty(evidence);

        EnsureInvariant(
            Status == VerificationStatus.Pending,
            "VERIFICATION_MUST_BE_PENDING",
            "Cannot add attempt to a non-pending verification.");

        EnsureInvariant(
            _attempts.Count < MaxAttempts,
            "MAX_ATTEMPTS_REACHED",
            $"Maximum attempts ({MaxAttempts}) reached.");

        var attempt = VerificationAttempt.Create(VerificationId.Value, evidence, _attempts.Count + 1, timestamp);
        _attempts.Add(attempt);

        RaiseDomainEvent(new VerificationAttemptedEvent(
            VerificationId.Value, IdentityId, attempt.AttemptNumber));

        return attempt;
    }

    public void Complete(DateTimeOffset timestamp)
    {
        EnsureInvariant(
            Status == VerificationStatus.Pending,
            "VERIFICATION_MUST_BE_PENDING",
            "Verification is not pending.");

        Status = VerificationStatus.Completed;
        CompletedAt = timestamp;

        RaiseDomainEvent(new VerificationCompletedEvent(VerificationId.Value, IdentityId));
    }

    public void Fail(string reason)
    {
        Guard.AgainstEmpty(reason);
        EnsureInvariant(
            Status == VerificationStatus.Pending,
            "VERIFICATION_MUST_BE_PENDING",
            "Verification is not pending.");

        Status = VerificationStatus.Failed;

        RaiseDomainEvent(new VerificationFailedEvent(VerificationId.Value, IdentityId, reason));
    }

    public void Expire()
    {
        EnsureInvariant(
            Status == VerificationStatus.Pending,
            "VERIFICATION_MUST_BE_PENDING",
            "Only pending verifications can expire.");

        Status = VerificationStatus.Expired;

        RaiseDomainEvent(new VerificationExpiredEvent(VerificationId.Value, IdentityId));
    }
}
