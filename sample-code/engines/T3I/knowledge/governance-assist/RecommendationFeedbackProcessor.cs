using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T3I.GovernanceAssist;

/// <summary>
/// Processes recommendation feedback (accepted/rejected) to update trust scoring inputs.
/// Feeds back into calibration signals for E12.1 integration.
/// Read-only of domain state, write-only to projection store.
/// </summary>
public sealed class RecommendationFeedbackProcessor
{
    private readonly IClock _clock;

    public RecommendationFeedbackProcessor(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>
    /// Process a recommendation outcome and produce calibration signals.
    /// </summary>
    public RecommendationFeedback Process(
        Guid recommendationId,
        bool wasAccepted,
        double predictedImpact,
        double? actualImpact)
    {
        var deviation = actualImpact.HasValue
            ? Math.Abs(predictedImpact - actualImpact.Value)
            : (double?)null;

        var accuracy = deviation.HasValue
            ? Math.Max(0.0, 1.0 - deviation.Value)
            : (double?)null;

        return new RecommendationFeedback
        {
            RecommendationId = recommendationId,
            WasAccepted = wasAccepted,
            PredictedImpact = predictedImpact,
            ActualImpact = actualImpact,
            DeviationScore = deviation,
            AccuracyScore = accuracy,
            RecordedAt = _clock.UtcNowOffset
        };
    }

    /// <summary>
    /// Compute drift stability from a set of feedback records.
    /// Higher stability = predictions closely match outcomes over time.
    /// </summary>
    public double ComputeDriftStability(IReadOnlyList<RecommendationFeedback> feedbackHistory)
    {
        var withActuals = feedbackHistory.Where(f => f.AccuracyScore.HasValue).ToList();
        if (withActuals.Count == 0) return 0.5; // neutral baseline

        return withActuals.Average(f => f.AccuracyScore!.Value);
    }
}

public sealed record RecommendationFeedback
{
    public required Guid RecommendationId { get; init; }
    public required bool WasAccepted { get; init; }
    public required double PredictedImpact { get; init; }
    public double? ActualImpact { get; init; }
    public double? DeviationScore { get; init; }
    public double? AccuracyScore { get; init; }
    public required DateTimeOffset RecordedAt { get; init; }
}
