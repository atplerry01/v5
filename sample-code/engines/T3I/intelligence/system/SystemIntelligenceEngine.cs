using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.Intelligence.System;

/// <summary>
/// T3I system intelligence engine. Aggregates cross-system health signals
/// and produces intelligence alerts. Stateless, deterministic, no domain imports.
/// Outputs insights — never mutates domain state.
/// </summary>
public sealed class SystemIntelligenceEngine : IEngine<SystemIntelligenceCommand>
{
    public Task<EngineResult> ExecuteAsync(
        SystemIntelligenceCommand command, EngineContext context, CancellationToken cancellationToken = default)
    {
        var alerts = new List<IntelligenceAlert>();

        // Evaluate system health metrics
        var healthThreshold = new AlertThreshold("system_health_score", 0.7m, 0.4m, "score");
        var healthSeverity = healthThreshold.Evaluate(1m - command.HealthScore); // Invert: lower score = worse
        if (healthSeverity >= AlertSeverity.Medium)
        {
            alerts.Add(new IntelligenceAlert(
                $"sys-health-{command.CorrelationId}",
                "SystemIntelligenceEngine",
                healthSeverity,
                ConfidenceScore.FromDeviation((1m - command.HealthScore) * 100m),
                "system.health",
                $"System health degraded: score={command.HealthScore:F2}",
                new Dictionary<string, string>
                {
                    ["system"] = command.SystemName,
                    ["score"] = command.HealthScore.ToString("F2")
                }));
        }

        // Evaluate error rate
        if (command.ErrorRate > 0.05m)
        {
            var errorSeverity = command.ErrorRate > 0.15m ? AlertSeverity.Critical : AlertSeverity.High;
            alerts.Add(new IntelligenceAlert(
                $"sys-errors-{command.CorrelationId}",
                "SystemIntelligenceEngine",
                errorSeverity,
                ConfidenceScore.High,
                "system.errors",
                $"Elevated error rate: {command.ErrorRate:P1}",
                new Dictionary<string, string>
                {
                    ["system"] = command.SystemName,
                    ["error_rate"] = command.ErrorRate.ToString("F4")
                }));
        }

        return Task.FromResult(EngineResult.Ok(new SystemIntelligenceResult
        {
            SystemName = command.SystemName,
            Alerts = alerts,
            OverallRisk = alerts.Count > 0 ? alerts.Max(a => a.Severity) : AlertSeverity.Info
        }));
    }
}

public sealed record SystemIntelligenceCommand
{
    public required string SystemName { get; init; }
    public required decimal HealthScore { get; init; }
    public required decimal ErrorRate { get; init; }
    public required long EventsProcessed { get; init; }
    public required string CorrelationId { get; init; }
}

public sealed record SystemIntelligenceResult
{
    public required string SystemName { get; init; }
    public required IReadOnlyList<IntelligenceAlert> Alerts { get; init; }
    public required AlertSeverity OverallRisk { get; init; }
}
