namespace Whycespace.Domain.IntelligenceSystem.Economic.Forecast;

public sealed record EconomicForecastGeneratedEvent(
    Guid ForecastId,
    Guid IdentityId,
    Guid? WalletId,
    string Scope,
    string ForecastType,
    string TimeHorizon,
    decimal PredictedValue,
    decimal ConfidenceScore,
    DateTimeOffset WindowStart,
    DateTimeOffset WindowEnd,
    DateTimeOffset ObservedAt,
    string TraceCorrelationId,
    string TraceSourceEventId
) : DomainEvent;
