namespace Whycespace.Domain.TrustSystem.Identity.Federation;

/// <summary>
/// Trust relationship between WhyceID and a federation issuer.
/// Tracks base trust, adjusted trust, trajectory, and status.
/// Scoring is deterministic: same inputs always produce same output.
/// </summary>
public sealed class FederationTrustAggregate : AggregateRoot
{
    public IssuerId IssuerId { get; private set; } = null!;
    public TrustLevel BaseTrustScore { get; private set; } = null!;
    public TrustLevel AdjustedTrustScore { get; private set; } = null!;
    public TrustTrajectory Trajectory { get; private set; } = null!;
    public DateTimeOffset LastEvaluatedAt { get; private set; }
    public FederationTrustStatus TrustStatus { get; private set; } = null!;

    private readonly List<decimal> _scoreHistory = [];

    private FederationTrustAggregate() { }

    public static FederationTrustAggregate Create(IssuerId issuerId, TrustLevel baseTrust, DateTimeOffset evaluatedAt)
    {
        Guard.AgainstNull(issuerId);
        Guard.AgainstNull(baseTrust);

        var trust = new FederationTrustAggregate
        {
            IssuerId = issuerId,
            BaseTrustScore = baseTrust,
            AdjustedTrustScore = baseTrust,
            Trajectory = TrustTrajectory.Initial(baseTrust.Value),
            LastEvaluatedAt = evaluatedAt,
            TrustStatus = FederationTrustStatus.Active
        };
        trust.Id = issuerId.Value;
        trust._scoreHistory.Add(baseTrust.Value);

        trust.RaiseDomainEvent(new TrustEvaluatedEvent(
            issuerId.Value, baseTrust.Value, baseTrust.Value,
            TrustTrend.Stable.ToString(), 0m));

        return trust;
    }

    /// <summary>
    /// Re-evaluate trust using formal, validated input objects.
    /// Deterministic: same inputs always produce same adjusted score.
    /// </summary>
    public void Evaluate(FederationTrustInput input, DateTimeOffset evaluatedAt)
    {
        Guard.AgainstNull(input);

        var previousScore = AdjustedTrustScore;

        // Deterministic trust computation using formal inputs
        decimal score = BaseTrustScore.Value;

        // Credential validity rate affects trust (scale 50–100% of base)
        score *= (0.5m + 0.5m * input.CredentialValidity.Value);

        // Revocation rate penalty
        score -= score * input.Revocation.Value * 0.3m;

        // Active link count bonus (more links = more confidence)
        score += Math.Min(input.Incident.ActiveLinkCount * 0.5m, 5m);

        // Incident penalty
        score -= input.Incident.IncidentCount * 3m;

        AdjustedTrustScore = new TrustLevel(Math.Clamp(score, 0m, 100m));
        LastEvaluatedAt = evaluatedAt;

        // Update score history and compute trajectory
        _scoreHistory.Add(AdjustedTrustScore.Value);
        var volatility = TrustTrajectory.ComputeVolatility(_scoreHistory);
        Trajectory = new TrustTrajectory(AdjustedTrustScore.Value, previousScore.Value, volatility);

        // Update status based on adjusted score
        TrustStatus = AdjustedTrustScore.Value switch
        {
            < 25m => FederationTrustStatus.Suspended,
            < 50m => FederationTrustStatus.Degraded,
            _ => FederationTrustStatus.Active
        };

        RaiseDomainEvent(new TrustEvaluatedEvent(
            IssuerId.Value, BaseTrustScore.Value, AdjustedTrustScore.Value,
            Trajectory.Trend.ToString(), Trajectory.Volatility));

        if (AdjustedTrustScore.Value < previousScore.Value)
        {
            RaiseDomainEvent(new TrustDegradedEvent(
                IssuerId.Value, previousScore.Value, AdjustedTrustScore.Value,
                $"Trust degraded from {previousScore.Value:F1} to {AdjustedTrustScore.Value:F1}"));
        }
    }
}

/// <summary>
/// Formal, validated inputs for deterministic trust evaluation.
/// Uses bounded value objects — no raw ints allowed.
/// </summary>
public sealed record FederationTrustInput
{
    public required CredentialValidityRate CredentialValidity { get; init; }
    public required RevocationRate Revocation { get; init; }
    public required IncidentRate Incident { get; init; }
}
