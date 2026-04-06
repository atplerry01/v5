namespace Whycespace.Engines.T3I.IdentityIntelligence;

/// <summary>
/// Results from identity intelligence engines (T3I).
/// Advisory only — never mutates domain state.
/// All results include ScoringVersion and explanations for auditability.
/// Uses string-based types instead of domain types.
/// </summary>
public sealed record TrustResult(
    string IdentityId,
    decimal Score,
    string Classification,
    string VersionId,
    string Explanation,
    DateTimeOffset ComputedAt);

public sealed record RiskResult(
    string IdentityId,
    decimal Score,
    string Severity,
    IReadOnlyList<AnomalyFlagDto> Flags,
    string VersionId,
    string Explanation,
    DateTimeOffset ComputedAt);

public sealed record AnomalyResult(
    string IdentityId,
    bool AnomalyDetected,
    IReadOnlyList<AnomalyFlagDto> Flags,
    DateTimeOffset DetectedAt);

public sealed record BehaviorResult(
    string IdentityId,
    IReadOnlyList<BehaviorSignalDto> Signals,
    string PatternHash,
    int EventCount,
    DateTimeOffset AnalyzedAt);

public sealed record GraphResult(
    string IdentityId,
    int NodeCount,
    int EdgeCount,
    IReadOnlyList<GraphNodeDto> Nodes,
    IReadOnlyList<GraphEdgeDto> Edges,
    DateTimeOffset BuiltAt);

public sealed record AnomalyFlagDto(
    string AnomalyType,
    string Description,
    decimal Confidence);
