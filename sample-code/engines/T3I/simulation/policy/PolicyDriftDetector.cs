using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T3I.Simulation.Policy;

/// <summary>
/// Detects divergence trends between predicted simulation outcomes and actual outcomes.
/// Accepts pre-fetched read model data — no repository dependencies.
/// </summary>
public sealed class PolicyDriftDetector
{
    private const int MinSamplesForDrift = 5;
    private const double DriftThreshold = 0.6;

    public DriftAssessment Detect(IReadOnlyList<SimulationOutcomeRecord> outcomes)
    {
        var calibrated = outcomes
            .Where(o => o.ActualOutcome is not null && o.DeviationScore.HasValue)
            .OrderByDescending(o => o.RecordedAt)
            .ToList();

        if (calibrated.Count < MinSamplesForDrift)
        {
            return new DriftAssessment(
                false, DriftType.None, 0.0,
                $"Insufficient data: {calibrated.Count} samples (minimum {MinSamplesForDrift} required).");
        }

        var overPredictions = 0;
        var underPredictions = 0;
        var exactMatches = 0;

        foreach (var outcome in calibrated)
        {
            var direction = ClassifyDeviationDirection(outcome.SimulationResult, outcome.ActualOutcome!);
            switch (direction)
            {
                case DeviationDirection.OverPredict: overPredictions++; break;
                case DeviationDirection.UnderPredict: underPredictions++; break;
                case DeviationDirection.Exact: exactMatches++; break;
            }
        }

        var total = calibrated.Count;
        var overRate = (double)overPredictions / total;
        var underRate = (double)underPredictions / total;

        if (overRate >= DriftThreshold)
        {
            return new DriftAssessment(
                true, DriftType.OverPrediction, overRate,
                $"Systematic over-prediction detected: simulation predicted Allow/pass {overPredictions}/{total} times " +
                "when actual outcome was Deny/fail. Model may overestimate permissiveness.");
        }

        if (underRate >= DriftThreshold)
        {
            return new DriftAssessment(
                true, DriftType.UnderPrediction, underRate,
                $"Systematic under-prediction detected: simulation predicted Deny/fail {underPredictions}/{total} times " +
                "when actual outcome was Allow/pass. Model may be overly restrictive.");
        }

        var recentDeviations = calibrated.Take(calibrated.Count / 2)
            .Where(o => o.DeviationScore.HasValue)
            .Select(o => o.DeviationScore!.Value)
            .ToList();
        var olderDeviations = calibrated.Skip(calibrated.Count / 2)
            .Where(o => o.DeviationScore.HasValue)
            .Select(o => o.DeviationScore!.Value)
            .ToList();

        if (recentDeviations.Count > 0 && olderDeviations.Count > 0)
        {
            var recentMean = recentDeviations.Average();
            var olderMean = olderDeviations.Average();

            if (recentMean > olderMean * 1.5 && recentMean > 0.3)
            {
                return new DriftAssessment(
                    true, DriftType.IncreasingDeviation, recentMean,
                    $"Increasing deviation trend: recent mean={recentMean:F2} vs older mean={olderMean:F2}. " +
                    "Model accuracy is degrading over time.");
            }
        }

        return new DriftAssessment(
            false, DriftType.None, 0.0,
            $"No drift detected across {total} samples. " +
            $"Exact: {exactMatches}, Over: {overPredictions}, Under: {underPredictions}.");
    }

    private static DeviationDirection ClassifyDeviationDirection(string predicted, string actual)
    {
        if (string.Equals(predicted, actual, StringComparison.OrdinalIgnoreCase))
            return DeviationDirection.Exact;

        if (predicted is "Allow" or "Conditional" && actual == "Deny")
            return DeviationDirection.OverPredict;

        if (predicted == "Deny" && actual is "Allow" or "Conditional")
            return DeviationDirection.UnderPredict;

        return DeviationDirection.Exact;
    }

    private enum DeviationDirection { Exact, OverPredict, UnderPredict }
}

public sealed record DriftAssessment(
    bool DriftDetected,
    DriftType DriftType,
    double DriftMagnitude,
    string Description);

public enum DriftType
{
    None,
    OverPrediction,
    UnderPrediction,
    IncreasingDeviation
}
