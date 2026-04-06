namespace Whycespace.Engines.T3I.EconomicAnomaly;

public sealed record DetectEconomicAnomalyCommand
{
    public required string AnomalyId { get; init; }
    public required string IdentityId { get; init; }
    public string? WalletId { get; init; }
    public required string Scope { get; init; }
    public required string CorrelationId { get; init; }
    public required string SourceEventId { get; init; }
    public required DateTimeOffset WindowStart { get; init; }
    public required DateTimeOffset WindowEnd { get; init; }

    // Actual (from analysis)
    public required decimal ActualVolume { get; init; }
    public required decimal ActualVelocity { get; init; }
    public required int ActualTransactionCount { get; init; }

    // Expected (from forecast)
    public required decimal ExpectedValue { get; init; }
    public required string ForecastType { get; init; }
}
