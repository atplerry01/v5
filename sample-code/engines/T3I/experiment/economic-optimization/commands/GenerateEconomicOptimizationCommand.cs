using Whycespace.Engines.T3I.EconomicAnomaly;
using Whycespace.Engines.T3I.EconomicForecast;

namespace Whycespace.Engines.T3I.EconomicOptimization;

public sealed record GenerateEconomicOptimizationCommand
{
    public required string OptimizationId { get; init; }
    public required string IdentityId { get; init; }
    public string? WalletId { get; init; }
    public required string Scope { get; init; }
    public required string CorrelationId { get; init; }
    public required string SourceEventId { get; init; }
    public required DateTimeOffset WindowStart { get; init; }
    public required DateTimeOffset WindowEnd { get; init; }

    // Analysis
    public required decimal Volume { get; init; }
    public required decimal Velocity { get; init; }
    public required int TransactionCount { get; init; }

    // Forecast
    public required decimal PredictedValue { get; init; }
    public required string ForecastType { get; init; }

    // Anomaly
    public required decimal DeviationPercentage { get; init; }
    public required string Severity { get; init; }
}
