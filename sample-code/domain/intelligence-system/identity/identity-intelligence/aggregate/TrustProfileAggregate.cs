namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// Intelligence trust profile — computed trust model for an identity.
/// Read-only intelligence aggregate. Does NOT mutate the domain TrustProfile.
///
/// Scoring model:
///   - Time decay: signals decay via exp(-lambda * ageInDays)
///   - Non-linear: diminishing returns on positive factors
///   - Anti-gaming: sudden improvement penalty applied
///   - Versioned: all parameters from explicit ScoringConfig
///   - Deterministic: same inputs + same config always produce same score
/// </summary>
public sealed class TrustProfileAggregate : AggregateRoot
{
    public Guid IdentityId { get; private set; }
    public TrustScore Score { get; private set; } = TrustScore.Zero;
    public TrustExplanation? LastExplanation { get; private set; }

    private TrustProfileAggregate() { }

    public static TrustProfileAggregate Create(Guid identityId)
    {
        Guard.AgainstDefault(identityId);
        var profile = new TrustProfileAggregate { IdentityId = identityId };
        profile.Id = identityId;
        return profile;
    }

    /// <summary>
    /// Recompute trust score using explicit versioned config.
    /// Deterministic: same inputs + same ScoringConfig always produce same score.
    /// </summary>
    public TrustScore Compute(TrustComputeInput input, ScoringConfig config, ScoringVersion version)
    {
        Guard.AgainstNull(input);
        Guard.AgainstNull(config);
        Guard.AgainstNull(version);

        var factors = new List<ExplanationFactor>();

        // 1. Verification level — diminishing returns
        var verificationContribution = DecayMath.DiminishingReturns(
            config.VerificationMaxContribution, input.VerificationLevel, config.VerificationRate);
        factors.Add(new ExplanationFactor("verification_level", verificationContribution,
            1.0m, input.VerificationLevel, "Verification level contribution (diminishing returns)"));

        // 2. Device trust — log scale
        var deviceContribution = DecayMath.LogScale(config.DeviceTrustWeight, input.DeviceTrustFactor);
        factors.Add(new ExplanationFactor("device_trust", deviceContribution,
            1.0m, input.DeviceTrustFactor, "Device trust factor (log scale)"));

        // 3. Behavior signals — time-decayed using config lambda
        decimal signalPenalty = 0m;
        foreach (var signal in input.BehaviorSignals)
        {
            var decayedWeight = DecayMath.DecayWeight(
                signal.Weight, signal.ObservedAt, input.EvaluatedAt, config.DecayLambda);
            signalPenalty += decayedWeight;
            factors.Add(new ExplanationFactor(
                $"signal:{signal.SignalType}", -decayedWeight,
                DecayMath.ExponentialDecay(
                    (input.EvaluatedAt - signal.ObservedAt).TotalDays, config.DecayLambda),
                signal.Weight,
                $"Behavior signal '{signal.SignalType}' (decay applied)"));
        }

        // 4. Policy violations — time-decayed with non-linear scaling
        decimal violationPenalty = 0m;
        foreach (var violation in input.PolicyViolations)
        {
            var decayed = DecayMath.DecayWeight(
                config.ViolationBaseWeight, violation.OccurredAt, input.EvaluatedAt, config.DecayLambda);
            violationPenalty += decayed;
        }
        var violationContribution = -DecayMath.LogScale(1m, violationPenalty);
        if (input.PolicyViolations.Count > 0)
        {
            factors.Add(new ExplanationFactor("policy_violations", violationContribution,
                1.0m, input.PolicyViolations.Count,
                $"{input.PolicyViolations.Count} violations (time-decayed, log scale)"));
        }

        // 5. Account age bonus — diminishing returns
        var ageContribution = DecayMath.DiminishingReturns(
            config.AccountAgeMaxContribution, input.AccountAgeDays, config.AccountAgeRate);
        factors.Add(new ExplanationFactor("account_age", ageContribution,
            1.0m, input.AccountAgeDays, "Account age bonus (diminishing returns)"));

        // 6. Anti-gaming: sudden improvement penalty (config-driven)
        decimal antiGamingPenalty = 0m;
        if (input.PreviousScore.HasValue && input.PreviousScoreAge.HasValue)
        {
            var rawScore = verificationContribution + deviceContribution
                - signalPenalty + violationContribution + ageContribution;
            var improvement = rawScore - input.PreviousScore.Value;
            var daysSinceLast = input.PreviousScoreAge.Value;

            if (improvement > config.AntiGamingImprovementThreshold
                && daysSinceLast < config.AntiGamingDaysThreshold)
            {
                antiGamingPenalty = improvement * config.AntiGamingPenaltyFactor;
                factors.Add(new ExplanationFactor("anti_gaming_sudden_improvement", -antiGamingPenalty,
                    1.0m, improvement,
                    $"Sudden improvement penalty: +{improvement:F1} in {daysSinceLast} days"));
            }
        }

        var totalScore = verificationContribution + deviceContribution - signalPenalty
            + violationContribution + ageContribution - antiGamingPenalty;

        Score = new TrustScore(totalScore);

        LastExplanation = new TrustExplanation(
            IdentityId.ToString(),
            Score.Value,
            Score.Classification,
            version,
            factors,
            input.EvaluatedAt);

        RaiseDomainEvent(new TrustCalculatedEvent(
            IdentityId, Score.Value, Score.Classification));

        return Score;
    }
}

public sealed record TrustComputeInput
{
    public required int VerificationLevel { get; init; }
    public required decimal DeviceTrustFactor { get; init; }
    public required IReadOnlyList<BehaviorSignal> BehaviorSignals { get; init; }
    public required IReadOnlyList<TimestampedViolation> PolicyViolations { get; init; }
    public required int AccountAgeDays { get; init; }
    public required DateTimeOffset EvaluatedAt { get; init; }
    public decimal? PreviousScore { get; init; }
    public int? PreviousScoreAge { get; init; }
}

public sealed record TimestampedViolation(string ViolationType, DateTimeOffset OccurredAt);

/// <summary>
/// Full explanation of a trust score computation.
/// Includes the ScoringVersion that produced it for reproducibility.
/// </summary>
public sealed record TrustExplanation(
    string IdentityId,
    decimal FinalScore,
    string Classification,
    ScoringVersion Version,
    IReadOnlyList<ExplanationFactor> Factors,
    DateTimeOffset ComputedAt);

public sealed record ExplanationFactor(
    string FactorName,
    decimal Contribution,
    decimal DecayApplied,
    decimal RawInput,
    string Description);
