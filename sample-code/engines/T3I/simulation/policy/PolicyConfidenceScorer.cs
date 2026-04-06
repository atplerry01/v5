using Whycespace.Engines.T3I.PolicySimulation;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T3I.Simulation.Policy;

/// <summary>
/// Computes confidence scores for simulation results based on:
/// - Historical deviation (calibration accuracy from pre-fetched data)
/// - Data completeness (resolved vs unresolved policies)
/// - Scenario richness (anomalies, risk, impact data)
/// No repository dependencies — accepts calibration metrics as parameters.
/// </summary>
public sealed class PolicyConfidenceScorer
{
    private readonly PolicyCalibrationEngine _calibrationEngine;

    public PolicyConfidenceScorer(PolicyCalibrationEngine calibrationEngine)
    {
        _calibrationEngine = calibrationEngine ?? throw new ArgumentNullException(nameof(calibrationEngine));
    }

    public ConfidenceAssessment Score(
        PolicySimulationResult result,
        IReadOnlyDictionary<Guid, IReadOnlyList<SimulationOutcomeRecord>> outcomesByPolicy)
    {
        ArgumentNullException.ThrowIfNull(result);

        var calibrationScore = ComputeCalibrationFactor(result, outcomesByPolicy);
        var completenessScore = ComputeCompletenessScore(result);
        var richnessScore = ComputeRichnessScore(result);

        var confidenceScore = Math.Clamp(
            (calibrationScore * 0.5) + (completenessScore * 0.3) + (richnessScore * 0.2),
            0.0,
            1.0);

        var level = confidenceScore switch
        {
            >= 0.75 => ConfidenceLevel.High,
            >= 0.40 => ConfidenceLevel.Medium,
            _ => ConfidenceLevel.Low
        };

        return new ConfidenceAssessment(
            Math.Round(confidenceScore, 4),
            level,
            new ConfidenceFactors(calibrationScore, completenessScore, richnessScore));
    }

    private double ComputeCalibrationFactor(
        PolicySimulationResult result,
        IReadOnlyDictionary<Guid, IReadOnlyList<SimulationOutcomeRecord>> outcomesByPolicy)
    {
        if (result.PolicyVersionsUsed.Count == 0)
            return 0.5;

        var totalAccuracy = 0.0;
        var count = 0;

        foreach (var policy in result.PolicyVersionsUsed)
        {
            if (outcomesByPolicy.TryGetValue(policy.PolicyId, out var outcomes))
            {
                var metrics = _calibrationEngine.ComputeCalibration(policy.PolicyId, outcomes);
                if (metrics.SampleCount > 0)
                {
                    totalAccuracy += metrics.AccuracyRate;
                    count++;
                }
            }
        }

        return count > 0 ? totalAccuracy / count : 0.5;
    }

    private static double ComputeCompletenessScore(PolicySimulationResult result)
    {
        var total = result.PolicyVersionsUsed.Count;
        if (total == 0) return 0.0;

        var resolved = result.DecisionSummary.PerPolicyDecisions
            .Count(d => d.Decision != "UNRESOLVED");
        return (double)resolved / total;
    }

    private static double ComputeRichnessScore(PolicySimulationResult result)
    {
        var score = 0.0;
        if (result.ImpactSummary is not null) score += 0.4;
        if (result.RiskScore is not null) score += 0.3;
        if (result.Anomalies.Count > 0) score += 0.15;
        if (result.Recommendation is not null) score += 0.15;
        return score;
    }
}

public sealed record ConfidenceAssessment(
    double ConfidenceScore,
    ConfidenceLevel ConfidenceLevel,
    ConfidenceFactors Factors);

public sealed record ConfidenceFactors(
    double CalibrationAccuracy,
    double DataCompleteness,
    double ScenarioRichness);

public enum ConfidenceLevel
{
    Low,
    Medium,
    High
}
