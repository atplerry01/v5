namespace Whycespace.Projections.GovernanceAssist;

/// <summary>
/// Read model storing recommendation outcomes for trust calibration feedback loop.
/// Written to projection store ONLY — never to transactional DB.
/// Feeds back into PolicyTrustScorer and RecommendationFeedbackProcessor.
/// </summary>
public sealed record RecommendationOutcomeProjection
{
    public required Guid Id { get; init; }
    public required Guid RecommendationId { get; init; }
    public required string Source { get; init; }
    public required IReadOnlyList<Guid> AffectedPolicies { get; init; }
    public required bool WasAccepted { get; init; }
    public required double PredictedImpact { get; init; }
    public double? ActualImpact { get; init; }
    public double? DeviationScore { get; init; }
    public double? AccuracyScore { get; init; }
    public required double ConfidenceAtGeneration { get; init; }
    public required int RiskAtGeneration { get; init; }
    public double? TrustScoreAtGeneration { get; init; }
    public bool? BiasDetectedAtGeneration { get; init; }
    public required DateTimeOffset GeneratedAt { get; init; }
    public required DateTimeOffset RecordedAt { get; init; }
}

/// <summary>
/// Aggregated recommendation reliability metrics.
/// </summary>
public sealed record RecommendationReliabilityProjection
{
    public required int TotalRecommendations { get; init; }
    public required int AcceptedCount { get; init; }
    public required int RejectedCount { get; init; }
    public required double AcceptanceRate { get; init; }
    public required double AverageAccuracy { get; init; }
    public required double AverageTrustScore { get; init; }
    public required int BiasDetectedCount { get; init; }
    public required int GuardrailViolationCount { get; init; }
    public required DateTimeOffset ComputedAt { get; init; }
}
