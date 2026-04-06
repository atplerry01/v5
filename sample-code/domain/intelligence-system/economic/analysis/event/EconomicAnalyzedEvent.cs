namespace Whycespace.Domain.IntelligenceSystem.Economic.Analysis;

public sealed record EconomicAnalyzedEvent(
    Guid AnalysisId,
    Guid IdentityId,
    Guid? WalletId,
    string Scope,
    decimal Volume,
    decimal Velocity,
    int TransactionCount,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    DateTimeOffset ObservedAt,
    string TraceCorrelationId,
    string TraceSourceEventId
) : DomainEvent;
