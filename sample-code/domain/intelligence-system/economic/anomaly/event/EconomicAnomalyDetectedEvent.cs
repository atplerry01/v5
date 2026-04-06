namespace Whycespace.Domain.IntelligenceSystem.Economic.Anomaly;

public sealed record EconomicAnomalyDetectedEvent(
    Guid AnomalyId,
    Guid IdentityId,
    Guid? WalletId,
    string Scope,
    decimal Deviation,
    decimal DeviationPercentage,
    string Severity,
    decimal ConfidenceScore,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    DateTimeOffset ObservedAt,
    string TraceCorrelationId,
    string TraceSourceEventId
) : DomainEvent;
