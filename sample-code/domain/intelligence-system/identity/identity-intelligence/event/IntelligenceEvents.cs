namespace Whycespace.Domain.IntelligenceSystem.Identity.IdentityIntelligence;

// whyce.identity-intelligence.trust.calculated
public sealed record TrustCalculatedEvent(
    Guid IdentityId, decimal Score, string Classification) : DomainEvent;

// whyce.identity-intelligence.risk.detected
public sealed record RiskDetectedEvent(
    Guid IdentityId, decimal Score, string Severity, IReadOnlyList<string> Flags) : DomainEvent;

// whyce.identity-intelligence.anomaly.flagged
public sealed record AnomalyFlaggedEvent(
    Guid IdentityId, string AnomalyType, string Description, decimal Confidence) : DomainEvent;

// whyce.identity-intelligence.anomaly.behavior-gaming-detected
public sealed record BehaviorGamingDetectedEvent(
    Guid IdentityId, string GamingType, string Description, decimal Confidence) : DomainEvent;

// whyce.identity-intelligence.graph.updated
public sealed record GraphUpdatedEvent(
    Guid IdentityId, string Action, string NodeType, string NodeId) : DomainEvent;

// whyce.identity-intelligence.behavior.analyzed
public sealed record BehaviorAnalyzedEvent(
    Guid IdentityId, IReadOnlyList<string> SignalTypes, string PatternHash) : DomainEvent;
