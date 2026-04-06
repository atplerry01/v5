namespace Whycespace.Engines.T3I.GovernanceAssist;

/// <summary>
/// Result of AGA analysis containing recommendations and system-level insights.
/// Immutable, advisory-only output.
/// </summary>
public sealed record GovernanceAssistResult
{
    public required Guid AnalysisId { get; init; }
    public required IReadOnlyList<RecommendationSummary> Recommendations { get; init; }
    public required IReadOnlyList<SystemInsight> Insights { get; init; }
    public OptimizationSummary? Optimization { get; init; }
    public required DateTimeOffset AnalyzedAt { get; init; }
    public required TimeSpan Duration { get; init; }
}

public sealed record RecommendationSummary
{
    public required Guid RecommendationId { get; init; }
    public required string Source { get; init; }
    public required IReadOnlyList<Guid> AffectedPolicies { get; init; }
    public required IReadOnlyList<string> SuggestedChanges { get; init; }
    public required double ConfidenceScore { get; init; }
    public required double CompositeImpact { get; init; }
    public required int RiskScore { get; init; }
    public required string RiskCategory { get; init; }
    public required IReadOnlyList<InsightSummary> Insights { get; init; }

    // --- E14.1 Hardening Extensions ---
    public TrustScoreSummary? TrustScore { get; init; }
    public bool BiasDetected { get; init; }
    public string? BiasType { get; init; }
    public string? BiasSeverity { get; init; }
    public bool RequiresManualReview { get; init; }
    public IReadOnlyList<string> GuardrailViolations { get; init; } = [];
    public ExplanationTreeSummary? Explanation { get; init; }
    public int? StrategyRank { get; init; }
}

public sealed record InsightSummary(string Type, string Description, string Severity);

public sealed record SystemInsight
{
    public required string Category { get; init; }
    public required string Description { get; init; }
    public required string Severity { get; init; }
    public required IReadOnlyList<Guid> RelatedPolicies { get; init; }
}

public sealed record OptimizationSummary
{
    public required IReadOnlyList<Guid> SelectedPolicies { get; init; }
    public required double AverageImpact { get; init; }
    public required int TotalRisk { get; init; }
}

public sealed record TrustScoreSummary(
    double HistoricalAccuracy,
    double AcceptanceRate,
    double DriftStability,
    double ConfidenceScore,
    double CompositeScore);

public sealed record ExplanationTreeSummary(
    string ReasoningSummary,
    IReadOnlyList<ExplanationNodeSummary> Nodes);

public sealed record ExplanationNodeSummary(
    string Label,
    string Category,
    string Detail,
    IReadOnlyList<ExplanationNodeSummary> Children);

public sealed record GovernanceProposalDraft
{
    public required Guid ProposalId { get; init; }
    public required Guid RecommendationId { get; init; }
    public required Guid PolicyId { get; init; }
    public required string VersionDiff { get; init; }
    public required string Rationale { get; init; }
    public required double ExpectedImpact { get; init; }
    public required double Confidence { get; init; }
    public required int Risk { get; init; }
    public required IReadOnlyList<string> SupportingEvidence { get; init; }
    public required DateTimeOffset GeneratedAt { get; init; }
}
