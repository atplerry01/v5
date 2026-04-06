using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.Intelligence.Anomaly;

/// <summary>
/// T3I anomaly pattern analyzer. Detects recurring anomaly patterns
/// across time windows. Stateless, deterministic computation.
/// Outputs pattern analysis — never mutates domain.
/// </summary>
public sealed class AnomalyPatternAnalyzer : IEngine<AnomalyPatternCommand>
{
    public Task<EngineResult> ExecuteAsync(
        AnomalyPatternCommand command, EngineContext context, CancellationToken cancellationToken = default)
    {
        var patterns = new List<DetectedPattern>();

        // Detect frequency patterns
        if (command.OccurrenceCount >= command.FrequencyThreshold)
        {
            patterns.Add(new DetectedPattern
            {
                PatternType = "HighFrequency",
                AnomalyType = command.AnomalyType,
                OccurrenceCount = command.OccurrenceCount,
                WindowDuration = command.WindowDuration,
                Confidence = ConfidenceScore.FromDeviation(
                    (decimal)command.OccurrenceCount / command.FrequencyThreshold * 25m),
                Severity = command.OccurrenceCount >= command.FrequencyThreshold * 2
                    ? AlertSeverity.Critical : AlertSeverity.High
            });
        }

        // Detect escalation pattern (increasing severity over time)
        if (command.PreviousSeverity < command.CurrentSeverity)
        {
            patterns.Add(new DetectedPattern
            {
                PatternType = "Escalating",
                AnomalyType = command.AnomalyType,
                OccurrenceCount = command.OccurrenceCount,
                WindowDuration = command.WindowDuration,
                Confidence = ConfidenceScore.Medium,
                Severity = command.CurrentSeverity
            });
        }

        return Task.FromResult(EngineResult.Ok(new AnomalyPatternResult
        {
            AnomalyType = command.AnomalyType,
            Patterns = patterns,
            RequiresGovernanceReview = patterns.Any(p => p.Severity >= AlertSeverity.High)
        }));
    }
}

public sealed record AnomalyPatternCommand
{
    public required string AnomalyType { get; init; }
    public required int OccurrenceCount { get; init; }
    public required int FrequencyThreshold { get; init; }
    public required TimeSpan WindowDuration { get; init; }
    public required AlertSeverity PreviousSeverity { get; init; }
    public required AlertSeverity CurrentSeverity { get; init; }
    public required string CorrelationId { get; init; }
}

public sealed record DetectedPattern
{
    public required string PatternType { get; init; }
    public required string AnomalyType { get; init; }
    public required int OccurrenceCount { get; init; }
    public required TimeSpan WindowDuration { get; init; }
    public required ConfidenceScore Confidence { get; init; }
    public required AlertSeverity Severity { get; init; }
}

public sealed record AnomalyPatternResult
{
    public required string AnomalyType { get; init; }
    public required IReadOnlyList<DetectedPattern> Patterns { get; init; }
    public required bool RequiresGovernanceReview { get; init; }
}
