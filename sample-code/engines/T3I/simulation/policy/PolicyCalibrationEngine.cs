using Whycespace.Shared.Contracts.Policy;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T3I.Simulation.Policy;

/// <summary>
/// Compares predicted simulation results against actual outcomes.
/// Computes deviation metrics from pre-fetched read model data.
/// Returns calibration results — does NOT write to any store.
/// </summary>
public sealed class PolicyCalibrationEngine
{
    private readonly IClock _clock;

    public PolicyCalibrationEngine(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>
    /// Produces a simulation outcome record for the caller to persist.
    /// </summary>
    public SimulationOutcomeRecord BuildSimulationOutcome(
        Guid policyId, int version, string simulationResult, Guid snapshotId)
    {
        return new SimulationOutcomeRecord
        {
            Id = DeterministicIdHelper.FromSeed($"SimOutcome:{policyId}:{version}:{snapshotId}:{simulationResult}"),
            PolicyId = policyId,
            Version = version,
            SimulationResult = simulationResult,
            SnapshotId = snapshotId,
            RecordedAt = _clock.UtcNowOffset
        };
    }

    /// <summary>
    /// Computes deviation between a prediction and its actual outcome.
    /// Returns an updated record for the caller to persist.
    /// </summary>
    public SimulationOutcomeRecord? ComputeActualOutcome(
        IReadOnlyList<SimulationOutcomeRecord> outcomes,
        int version, string actualOutcome, Guid snapshotId)
    {
        var matchingPrediction = outcomes
            .Where(o => o.Version == version && o.SnapshotId == snapshotId && o.ActualOutcome is null)
            .OrderByDescending(o => o.RecordedAt)
            .FirstOrDefault();

        if (matchingPrediction is null)
            return null;

        var deviation = ComputeDeviation(matchingPrediction.SimulationResult, actualOutcome);

        return matchingPrediction with
        {
            ActualOutcome = actualOutcome,
            DeviationScore = deviation
        };
    }

    /// <summary>
    /// Computes calibration metrics from pre-fetched outcome read model data.
    /// </summary>
    public CalibrationMetrics ComputeCalibration(
        Guid policyId, IReadOnlyList<SimulationOutcomeRecord> outcomes)
    {
        var calibrated = outcomes
            .Where(o => o.ActualOutcome is not null && o.DeviationScore.HasValue)
            .ToList();

        if (calibrated.Count == 0)
        {
            return new CalibrationMetrics(
                policyId, 0, 0, 0, 0, 0, _clock.UtcNowOffset);
        }

        var deviations = calibrated.Select(o => o.DeviationScore!.Value).OrderBy(d => d).ToList();
        var meanDeviation = deviations.Average();
        var medianDeviation = deviations[deviations.Count / 2];
        var maxDeviation = deviations.Max();
        var accuracyRate = calibrated.Count(o => o.DeviationScore!.Value < AccuracyThreshold)
                           / (double)calibrated.Count;

        return new CalibrationMetrics(
            policyId,
            calibrated.Count,
            meanDeviation,
            medianDeviation,
            maxDeviation,
            accuracyRate,
            _clock.UtcNowOffset);
    }

    private static double ComputeDeviation(string predicted, string actual)
    {
        if (string.Equals(predicted, actual, StringComparison.OrdinalIgnoreCase))
            return 0.0;

        if (predicted != "Deny" && actual != "Deny")
            return 0.5;

        return 1.0;
    }

    private const double AccuracyThreshold = 0.5;
}

public sealed record CalibrationMetrics(
    Guid PolicyId,
    int SampleCount,
    double MeanDeviation,
    double MedianDeviation,
    double MaxDeviation,
    double AccuracyRate,
    DateTimeOffset ComputedAt);
