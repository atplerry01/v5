using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.Intelligence.Predictive;

/// <summary>
/// T3I predictive failure analyzer. Uses trend analysis on historical
/// metrics to predict upcoming failures. Stateless, deterministic.
/// Outputs predictions — never mutates domain.
/// </summary>
public sealed class PredictiveFailureAnalyzer : IEngine<PredictiveFailureCommand>
{
    public Task<EngineResult> ExecuteAsync(
        PredictiveFailureCommand command, EngineContext context, CancellationToken cancellationToken = default)
    {
        var predictions = new List<FailurePrediction>();

        // Linear trend projection
        if (command.DataPoints.Count >= 3)
        {
            var trend = CalculateTrend(command.DataPoints);

            // Project forward to estimate time-to-threshold
            if (trend.Slope > 0 && command.FailureThreshold > 0)
            {
                var currentValue = command.DataPoints[^1].Value;
                var remaining = command.FailureThreshold - currentValue;

                if (trend.Slope > 0 && remaining > 0)
                {
                    var intervalsToFailure = remaining / trend.Slope;
                    var estimatedInterval = command.DataPoints.Count > 1
                        ? (command.DataPoints[^1].SequenceIndex - command.DataPoints[0].SequenceIndex)
                          / (decimal)(command.DataPoints.Count - 1)
                        : 1m;

                    predictions.Add(new FailurePrediction
                    {
                        MetricName = command.MetricName,
                        CurrentValue = currentValue,
                        FailureThreshold = command.FailureThreshold,
                        TrendSlope = trend.Slope,
                        EstimatedIntervalsToFailure = intervalsToFailure,
                        Confidence = ConfidenceScore.FromDeviation(trend.R2 * 100m),
                        Severity = intervalsToFailure < 5 ? AlertSeverity.Critical
                            : intervalsToFailure < 10 ? AlertSeverity.High
                            : AlertSeverity.Medium
                    });
                }
            }
        }

        return Task.FromResult(EngineResult.Ok(new PredictiveFailureResult
        {
            MetricName = command.MetricName,
            Predictions = predictions,
            RequiresPreemptiveAction = predictions.Any(p => p.Severity >= AlertSeverity.High)
        }));
    }

    private static TrendResult CalculateTrend(IReadOnlyList<MetricDataPoint> points)
    {
        var n = points.Count;
        decimal sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0, sumY2 = 0;

        for (var i = 0; i < n; i++)
        {
            var x = points[i].SequenceIndex;
            var y = points[i].Value;
            sumX += x; sumY += y; sumXY += x * y; sumX2 += x * x; sumY2 += y * y;
        }

        var denom = n * sumX2 - sumX * sumX;
        if (denom == 0) return new TrendResult(0, 0);

        var slope = (n * sumXY - sumX * sumY) / denom;
        var ssTot = n * sumY2 - sumY * sumY;
        var ssRes = ssTot - slope * slope * denom;
        var r2 = ssTot == 0 ? 0 : Math.Max(0, 1m - ssRes / ssTot);

        return new TrendResult(slope, r2);
    }

    private sealed record TrendResult(decimal Slope, decimal R2);
}

public sealed record PredictiveFailureCommand
{
    public required string MetricName { get; init; }
    public required IReadOnlyList<MetricDataPoint> DataPoints { get; init; }
    public required decimal FailureThreshold { get; init; }
    public required string CorrelationId { get; init; }
}

public sealed record MetricDataPoint(decimal SequenceIndex, decimal Value);

public sealed record FailurePrediction
{
    public required string MetricName { get; init; }
    public required decimal CurrentValue { get; init; }
    public required decimal FailureThreshold { get; init; }
    public required decimal TrendSlope { get; init; }
    public required decimal EstimatedIntervalsToFailure { get; init; }
    public required ConfidenceScore Confidence { get; init; }
    public required AlertSeverity Severity { get; init; }
}

public sealed record PredictiveFailureResult
{
    public required string MetricName { get; init; }
    public required IReadOnlyList<FailurePrediction> Predictions { get; init; }
    public required bool RequiresPreemptiveAction { get; init; }
}
