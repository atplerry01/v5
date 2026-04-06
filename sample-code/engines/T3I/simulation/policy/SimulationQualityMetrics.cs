using Whycespace.Engines.T3I.Simulation.Policy;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T3I.PolicySimulation.Metrics;

/// <summary>
/// Emits simulation quality metrics for observability.
/// Metrics: simulation.accuracy, simulation.deviation, simulation.confidence, simulation.drift_detected.
/// Stateless — computes from result + calibration data. No side effects.
/// </summary>
public sealed class SimulationQualityMetrics
{
    private readonly IClock _clock;

    public SimulationQualityMetrics(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public SimulationQualityReport Compute(
        PolicySimulationResult result,
        CalibrationMetrics? calibration,
        DriftAssessment? drift)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new SimulationQualityReport
        {
            ScenarioId = result.ScenarioId,
            Accuracy = calibration?.AccuracyRate ?? 0.0,
            MeanDeviation = calibration?.MeanDeviation ?? 0.0,
            ConfidenceScore = result.Confidence?.ConfidenceScore ?? 0.0,
            ConfidenceLevel = result.Confidence?.ConfidenceLevel.ToString() ?? "UNKNOWN",
            DriftDetected = drift?.DriftDetected ?? false,
            DriftType = drift?.DriftType.ToString() ?? "None",
            DriftMagnitude = drift?.DriftMagnitude ?? 0.0,
            PoliciesEvaluated = result.DecisionSummary.PoliciesEvaluated,
            RulesEvaluated = result.DecisionSummary.RulesEvaluated,
            RulesPassed = result.DecisionSummary.RulesPassed,
            RulesFailed = result.DecisionSummary.RulesFailed,
            AnomalyCount = result.Anomalies.Count,
            OverallDecision = result.DecisionSummary.OverallDecision,
            Duration = result.Duration,
            EmittedAt = _clock.UtcNowOffset
        };
    }
}

public sealed record SimulationQualityReport
{
    // simulation.accuracy
    public required double Accuracy { get; init; }

    // simulation.deviation
    public required double MeanDeviation { get; init; }

    // simulation.confidence
    public required double ConfidenceScore { get; init; }
    public required string ConfidenceLevel { get; init; }

    // simulation.drift_detected
    public required bool DriftDetected { get; init; }
    public required string DriftType { get; init; }
    public required double DriftMagnitude { get; init; }

    // context
    public required Guid ScenarioId { get; init; }
    public required int PoliciesEvaluated { get; init; }
    public required int RulesEvaluated { get; init; }
    public required int RulesPassed { get; init; }
    public required int RulesFailed { get; init; }
    public required int AnomalyCount { get; init; }
    public required string OverallDecision { get; init; }
    public required TimeSpan Duration { get; init; }
    public required DateTimeOffset EmittedAt { get; init; }
}
