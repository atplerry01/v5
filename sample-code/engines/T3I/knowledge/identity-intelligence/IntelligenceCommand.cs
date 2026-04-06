namespace Whycespace.Engines.T3I.IdentityIntelligence;

/// <summary>
/// Commands for identity intelligence engines (T3I).
/// All engines are stateless, read-only, and produce advisory outputs.
/// ALL inputs MUST be chain-verified — unverified events are rejected.
/// </summary>
public abstract record IntelligenceCommand;

public sealed record ComputeTrustCommand(
    string IdentityId,
    int VerificationLevel,
    decimal DeviceTrustFactor,
    IReadOnlyList<BehaviorSignalDto> BehaviorSignals,
    IReadOnlyList<ViolationDto> PolicyViolations,
    int AccountAgeDays,
    DateTimeOffset EvaluatedAt,
    bool ChainVerified,
    string ScoringVersionId = "v1.0",
    decimal? PreviousScore = null,
    int? PreviousScoreAgeDays = null) : IntelligenceCommand;

public sealed record ComputeRiskCommand(
    string IdentityId,
    int FailedAuthCount,
    int DeviceSwitchesInWindow,
    decimal LoginFrequencyRatio,
    decimal GraphAnomalyScore,
    IReadOnlyList<ViolationDto> PolicyViolations,
    IReadOnlyList<BehaviorSignalDto> BehaviorSignals,
    DateTimeOffset EvaluatedAt,
    bool ChainVerified,
    string ScoringVersionId = "v1.0") : IntelligenceCommand;

public sealed record DetectAnomaliesCommand(
    string IdentityId,
    IReadOnlyList<LoginEventDto> RecentLogins,
    IReadOnlyList<DeviceEventDto> RecentDeviceEvents,
    int GraphNodeCount,
    int GraphEdgeCount,
    bool ChainVerified,
    IReadOnlyList<TrustScoreHistoryDto>? TrustHistory = null) : IntelligenceCommand;

public sealed record AnalyzeBehaviorCommand(
    string IdentityId,
    IReadOnlyList<SessionEventDto> SessionEvents,
    IReadOnlyList<string> RecentEventTypes) : IntelligenceCommand;

public sealed record BuildGraphCommand(
    string IdentityId,
    IReadOnlyList<GraphNodeDto> Nodes,
    IReadOnlyList<GraphEdgeDto> Edges) : IntelligenceCommand;

// -- DTOs --

public sealed record BehaviorSignalDto(string SignalType, decimal Weight, DateTimeOffset ObservedAt);

public sealed record ViolationDto(string ViolationType, DateTimeOffset OccurredAt);

public sealed record LoginEventDto(string IdentityId, string DeviceId, DateTimeOffset Timestamp, bool Success);

public sealed record DeviceEventDto(string DeviceId, string EventType, DateTimeOffset Timestamp);

public sealed record SessionEventDto(string SessionId, string IdentityId, DateTimeOffset StartedAt, DateTimeOffset? EndedAt);

public sealed record GraphNodeDto(string NodeId, string NodeType, string Status);

public sealed record GraphEdgeDto(string SourceNodeId, string TargetNodeId, string EdgeType, decimal Strength);

public sealed record TrustScoreHistoryDto(decimal Score, DateTimeOffset ComputedAt);
