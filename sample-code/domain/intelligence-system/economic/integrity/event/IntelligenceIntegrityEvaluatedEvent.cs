namespace Whycespace.Domain.IntelligenceSystem.Economic.Integrity;

public sealed record IntelligenceIntegrityEvaluatedEvent(
    Guid IntegrityId,
    Guid IdentityId,
    Guid? WalletId,
    string Scope,
    decimal IntegrityScore,
    decimal CalibratedConfidence,
    bool ConflictDetected,
    string? ConflictReason,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    DateTimeOffset ObservedAt,
    string TraceCorrelationId
) : DomainEvent;
