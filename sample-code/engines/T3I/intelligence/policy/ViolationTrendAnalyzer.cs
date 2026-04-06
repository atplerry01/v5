using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.Intelligence.Policy;

/// <summary>
/// T3I violation trend analyzer. Analyzes policy violation patterns
/// to identify systemic issues. Stateless, deterministic.
/// Outputs trend analysis — never mutates domain.
/// </summary>
public sealed class ViolationTrendAnalyzer : IEngine<ViolationTrendCommand>
{
    public Task<EngineResult> ExecuteAsync(
        ViolationTrendCommand command, EngineContext context, CancellationToken cancellationToken = default)
    {
        var violationRate = command.WindowDurationHours > 0
            ? (decimal)command.ViolationCount / command.WindowDurationHours
            : 0m;

        var threshold = AlertThresholdRegistry.PolicyViolationRate;
        var severity = threshold.Evaluate(violationRate);

        var trend = command.PreviousViolationCount > 0
            ? (decimal)command.ViolationCount / command.PreviousViolationCount
            : 1m;

        var isEscalating = trend > 1.5m;

        return Task.FromResult(EngineResult.Ok(new ViolationTrendResult
        {
            PolicyId = command.PolicyId,
            ViolationRate = violationRate,
            TrendMultiplier = trend,
            IsEscalating = isEscalating,
            Severity = severity,
            SuggestedAction = severity >= AlertSeverity.High
                ? "Recommend policy threshold adjustment or additional controls"
                : isEscalating
                    ? "Monitor — escalating trend detected"
                    : "No action required"
        }));
    }
}

public sealed record ViolationTrendCommand
{
    public required string PolicyId { get; init; }
    public required int ViolationCount { get; init; }
    public required int PreviousViolationCount { get; init; }
    public required decimal WindowDurationHours { get; init; }
    public required string CorrelationId { get; init; }
}

public sealed record ViolationTrendResult
{
    public required string PolicyId { get; init; }
    public required decimal ViolationRate { get; init; }
    public required decimal TrendMultiplier { get; init; }
    public required bool IsEscalating { get; init; }
    public required AlertSeverity Severity { get; init; }
    public required string SuggestedAction { get; init; }
}
