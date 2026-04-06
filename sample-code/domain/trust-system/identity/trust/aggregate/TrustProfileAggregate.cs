using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.TrustSystem.Identity.Trust;

public sealed class TrustProfileAggregate : AggregateRoot
{
    public TrustProfileId TrustProfileId { get; private set; } = null!;
    public Guid IdentityId { get; private set; }
    public TrustScoreValue Score { get; private set; } = null!;
    public TrustLevel Level { get; private set; } = null!;
    public TrustProfileStatus Status { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastEvaluatedAt { get; private set; }

    private readonly List<TrustHistory> _history = [];
    public IReadOnlyList<TrustHistory> History => _history.AsReadOnly();

    private TrustProfileAggregate() { }

    public static TrustProfileAggregate Initialize(Guid identityId, DateTimeOffset timestamp)
    {
        Guard.AgainstDefault(identityId);

        var profile = new TrustProfileAggregate
        {
            TrustProfileId = TrustProfileId.FromSeed($"TrustProfile:{identityId}"),
            IdentityId = identityId,
            Score = TrustScoreValue.Initial,
            Level = TrustLevel.Unverified,
            Status = TrustProfileStatus.Active,
            CreatedAt = timestamp,
            LastEvaluatedAt = timestamp
        };

        profile.Id = profile.TrustProfileId.Value;
        profile.RaiseDomainEvent(new TrustProfileInitializedEvent(
            profile.TrustProfileId.Value, identityId));
        return profile;
    }

    public void RecordFactor(TrustFactor factor, decimal weight, DateTimeOffset timestamp)
    {
        Guard.AgainstNull(factor);
        EnsureInvariant(
            Status == TrustProfileStatus.Active,
            "TRUST_MUST_BE_ACTIVE",
            "Cannot record factors on a frozen or closed profile.");

        var entry = TrustHistory.Record(TrustProfileId.Value, factor, weight, Score.Value, timestamp);
        _history.Add(entry);

        var newScore = TrustScoreValue.Compute(Score, weight);
        var oldScore = Score;
        Score = newScore;
        Level = TrustLevel.FromScore(newScore.Value);
        LastEvaluatedAt = timestamp;

        RaiseDomainEvent(new TrustFactorRecordedEvent(
            TrustProfileId.Value, IdentityId, factor.Value, weight));

        if (oldScore.Value != newScore.Value)
        {
            RaiseDomainEvent(new TrustScoreUpdatedEvent(
                TrustProfileId.Value, IdentityId, oldScore.Value, newScore.Value, Level.Value));
        }
    }

    public void Freeze(string reason)
    {
        Guard.AgainstEmpty(reason);
        EnsureInvariant(
            Status == TrustProfileStatus.Active,
            "TRUST_MUST_BE_ACTIVE",
            "Profile is not active.");

        Status = TrustProfileStatus.Frozen;
        RaiseDomainEvent(new TrustProfileFrozenEvent(TrustProfileId.Value, IdentityId, reason));
    }

    public void Unfreeze()
    {
        EnsureInvariant(
            Status == TrustProfileStatus.Frozen,
            "TRUST_MUST_BE_FROZEN",
            "Profile is not frozen.");

        Status = TrustProfileStatus.Active;
        RaiseDomainEvent(new TrustProfileUnfrozenEvent(TrustProfileId.Value, IdentityId));
    }
}
