namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

/// <summary>
/// Intelligence risk profile — computed risk model for an identity.
/// Versioned, time-decayed, non-linear, explainable.
/// Deterministic: same inputs + same ScoringConfig always produce same score.
/// </summary>
public sealed class RiskProfileAggregate : AggregateRoot
{
    public Guid IdentityId { get; private set; }
    public RiskScore Score { get; private set; } = RiskScore.None;
    public IReadOnlyList<AnomalyFlag> ActiveFlags => _flags.AsReadOnly();
    public RiskExplanation? LastExplanation { get; private set; }

    private readonly List<AnomalyFlag> _flags = [];

    private RiskProfileAggregate() { }

    public static RiskProfileAggregate Create(Guid identityId)
    {
        Guard.AgainstDefault(identityId);
        var profile = new RiskProfileAggregate { IdentityId = identityId };
        profile.Id = identityId;
        return profile;
    }

    /// <summary>
    /// Compute risk score using explicit versioned config.
    /// Deterministic: same inputs + same ScoringConfig always produce same score.
    /// </summary>
    public RiskScore Compute(RiskComputeInput input, ScoringConfig config, ScoringVersion version)
    {
        Guard.AgainstNull(input);
        Guard.AgainstNull(config);
        Guard.AgainstNull(version);

        var factors = new List<ExplanationFactor>();
        _flags.Clear();

        // 1. Failed auth — non-linear (log scale)
        decimal failedAuthContribution = 0m;
        if (input.FailedAuthCount > config.FailedAuthThreshold)
        {
            failedAuthContribution = DecayMath.LogScale(config.FailedAuthWeight, input.FailedAuthCount);
            factors.Add(new ExplanationFactor("failed_auth", failedAuthContribution, 1.0m,
                input.FailedAuthCount, $"Failed auth: {input.FailedAuthCount} attempts (log scale)"));
            _flags.Add(new AnomalyFlag(
                AnomalyFlag.Types.AccessPatternAnomaly,
                $"High failed auth count: {input.FailedAuthCount}",
                Math.Min(0.5m + DecayMath.LogScale(0.3m, input.FailedAuthCount), 1m),
                input.EvaluatedAt));
        }

        // 2. Device switching — diminishing returns
        decimal deviceSwitchContribution = 0m;
        if (input.DeviceSwitchesInWindow > config.DeviceSwitchThreshold)
        {
            deviceSwitchContribution = DecayMath.DiminishingReturns(
                config.DeviceSwitchMaxContribution, input.DeviceSwitchesInWindow, config.DeviceSwitchRate);
            factors.Add(new ExplanationFactor("device_switching", deviceSwitchContribution, 1.0m,
                input.DeviceSwitchesInWindow,
                $"Device switching: {input.DeviceSwitchesInWindow} (diminishing returns)"));
            _flags.Add(new AnomalyFlag(
                AnomalyFlag.Types.DeviceSwitchingAnomaly,
                $"Rapid device switching: {input.DeviceSwitchesInWindow} in window",
                0.7m, input.EvaluatedAt));
        }

        // 3. Login frequency — non-linear
        decimal loginFreqContribution = 0m;
        if (input.LoginFrequencyRatio > config.LoginFreqThreshold)
        {
            loginFreqContribution = DecayMath.LogScale(config.LoginFreqWeight, input.LoginFrequencyRatio);
            factors.Add(new ExplanationFactor("login_frequency", loginFreqContribution, 1.0m,
                input.LoginFrequencyRatio,
                $"Login frequency {input.LoginFrequencyRatio:F1}x above baseline (log scale)"));
            _flags.Add(new AnomalyFlag(
                AnomalyFlag.Types.UnusualLoginFrequency,
                $"Login frequency {input.LoginFrequencyRatio:F1}x above baseline",
                0.6m, input.EvaluatedAt));
        }

        // 4. Graph anomalies
        decimal graphContribution = input.GraphAnomalyScore;
        if (graphContribution > 0)
        {
            factors.Add(new ExplanationFactor("graph_anomaly", graphContribution, 1.0m,
                graphContribution, "Graph anomaly score"));
        }

        // 5. Policy violations — time-decayed using config lambda
        decimal violationContribution = 0m;
        foreach (var violation in input.PolicyViolations)
        {
            var decayed = DecayMath.DecayWeight(
                config.ViolationRiskWeight, violation.OccurredAt, input.EvaluatedAt, config.DecayLambda);
            violationContribution += decayed;
        }
        if (input.PolicyViolations.Count > 0)
        {
            factors.Add(new ExplanationFactor("policy_violations", violationContribution,
                1.0m, input.PolicyViolations.Count,
                $"{input.PolicyViolations.Count} violations (time-decayed)"));
        }

        // 6. Behavioral signals — time-decayed
        decimal signalContribution = 0m;
        foreach (var signal in input.BehaviorSignals)
        {
            if (signal.Weight > 0)
            {
                var decayed = DecayMath.DecayWeight(
                    signal.Weight, signal.ObservedAt, input.EvaluatedAt, config.DecayLambda);
                signalContribution += decayed;
                factors.Add(new ExplanationFactor(
                    $"signal:{signal.SignalType}", decayed,
                    DecayMath.ExponentialDecay(
                        (input.EvaluatedAt - signal.ObservedAt).TotalDays, config.DecayLambda),
                    signal.Weight,
                    $"Signal '{signal.SignalType}' (decay applied)"));
            }
        }

        var totalScore = failedAuthContribution + deviceSwitchContribution
            + loginFreqContribution + graphContribution
            + violationContribution + signalContribution;

        Score = new RiskScore(totalScore);

        LastExplanation = new RiskExplanation(
            IdentityId.ToString(),
            Score.Value,
            Score.Severity,
            version,
            _flags.Select(f => f.AnomalyType).ToList(),
            factors,
            input.EvaluatedAt);

        if (Score.Value > 0)
        {
            var flagNames = _flags.Select(f => f.AnomalyType).ToList();
            RaiseDomainEvent(new RiskDetectedEvent(
                IdentityId, Score.Value, Score.Severity, flagNames));
        }

        return Score;
    }

    public void AddFlag(AnomalyFlag flag)
    {
        Guard.AgainstNull(flag);
        _flags.Add(flag);
        RaiseDomainEvent(new AnomalyFlaggedEvent(
            IdentityId, flag.AnomalyType, flag.Description, flag.Confidence));
    }
}

public sealed record RiskComputeInput
{
    public required int FailedAuthCount { get; init; }
    public required int DeviceSwitchesInWindow { get; init; }
    public required decimal LoginFrequencyRatio { get; init; }
    public required decimal GraphAnomalyScore { get; init; }
    public required IReadOnlyList<TimestampedViolation> PolicyViolations { get; init; }
    public required IReadOnlyList<BehaviorSignal> BehaviorSignals { get; init; }
    public required DateTimeOffset EvaluatedAt { get; init; }
}

/// <summary>
/// Full explanation of a risk score computation.
/// Includes the ScoringVersion that produced it for reproducibility.
/// </summary>
public sealed record RiskExplanation(
    string IdentityId,
    decimal FinalScore,
    string Severity,
    ScoringVersion Version,
    IReadOnlyList<string> AnomalyFlags,
    IReadOnlyList<ExplanationFactor> Factors,
    DateTimeOffset ComputedAt);
