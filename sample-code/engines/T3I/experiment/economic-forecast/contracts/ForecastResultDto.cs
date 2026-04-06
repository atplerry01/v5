namespace Whycespace.Engines.T3I.EconomicForecast;

public sealed record ForecastResultDto
{
    public required string ForecastId { get; init; }
    public required string IdentityId { get; init; }
    public string? WalletId { get; init; }
    public required string Scope { get; init; }
    public required string ForecastType { get; init; }
    public required string TimeHorizon { get; init; }
    public required decimal PredictedValue { get; init; }
    public required decimal ConfidenceScore { get; init; }
    public required DateTimeOffset WindowStart { get; init; }
    public required DateTimeOffset WindowEnd { get; init; }
    public required DateTimeOffset ObservedAt { get; init; }
    public required string CorrelationId { get; init; }
    public required string SourceEventId { get; init; }
}
