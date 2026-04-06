namespace Whycespace.Engines.T3I.IntelligenceIntegrity;

public sealed record EvaluateIntelligenceIntegrityCommand
{
    public required string IntegrityId { get; init; }
    public required string IdentityId { get; init; }
    public string? WalletId { get; init; }
    public required string Scope { get; init; }
    public required string CorrelationId { get; init; }

    // Analysis
    public required decimal Volume { get; init; }
    public required decimal Velocity { get; init; }
    public required int TransactionCount { get; init; }

    // Forecast
    public required decimal PredictedValue { get; init; }
    public required decimal ForecastConfidence { get; init; }

    // Anomaly
    public required decimal DeviationPercentage { get; init; }
    public required decimal AnomalyConfidence { get; init; }

    // Optimization
    public required decimal ExpectedImpact { get; init; }
    public required decimal OptimizationConfidence { get; init; }

    public required DateTimeOffset WindowStart { get; init; }
    public required DateTimeOffset WindowEnd { get; init; }
}
