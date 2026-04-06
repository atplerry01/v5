namespace Whycespace.Domain.IntelligenceSystem.Economic.Optimization;

public sealed record EconomicRecommendationGeneratedEvent(
    Guid OptimizationId,
    Guid IdentityId,
    Guid? WalletId,
    string Scope,
    string RecommendationType,
    string SuggestedAction,
    decimal ExpectedImpact,
    decimal ConfidenceScore,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    DateTimeOffset ObservedAt,
    string TraceCorrelationId,
    string TraceSourceEventId
) : DomainEvent;
