using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.Intelligence.Performance;

/// <summary>
/// T3I performance analyzer. Evaluates system throughput, latency,
/// and resource utilization. Stateless, deterministic.
/// Outputs performance insights — never mutates domain.
/// </summary>
public sealed class PerformanceAnalyzer : IEngine<PerformanceAnalysisCommand>
{
    public Task<EngineResult> ExecuteAsync(
        PerformanceAnalysisCommand command, EngineContext context, CancellationToken cancellationToken = default)
    {
        var insights = new List<PerformanceInsight>();

        // Latency analysis
        if (command.P99LatencyMs > command.LatencyBudgetMs)
        {
            var overage = command.P99LatencyMs / command.LatencyBudgetMs;
            insights.Add(new PerformanceInsight
            {
                Category = "Latency",
                Metric = "p99_latency_ms",
                CurrentValue = command.P99LatencyMs,
                Threshold = command.LatencyBudgetMs,
                Severity = overage > 2m ? AlertSeverity.Critical : AlertSeverity.High,
                Recommendation = overage > 2m
                    ? "Consider scaling or circuit-breaking degraded services"
                    : "Review slow query paths and cache hit rates"
            });
        }

        // Throughput analysis
        if (command.CurrentThroughput < command.ExpectedThroughput * 0.7m)
        {
            insights.Add(new PerformanceInsight
            {
                Category = "Throughput",
                Metric = "commands_per_second",
                CurrentValue = command.CurrentThroughput,
                Threshold = command.ExpectedThroughput,
                Severity = AlertSeverity.High,
                Recommendation = "Investigate bottleneck: check middleware latency and engine concurrency"
            });
        }

        // Resource utilization
        if (command.CpuUtilization > 0.85m)
        {
            insights.Add(new PerformanceInsight
            {
                Category = "Resource",
                Metric = "cpu_utilization",
                CurrentValue = command.CpuUtilization,
                Threshold = 0.85m,
                Severity = command.CpuUtilization > 0.95m ? AlertSeverity.Critical : AlertSeverity.Medium,
                Recommendation = "Scale horizontally or optimize compute-heavy operations"
            });
        }

        return Task.FromResult(EngineResult.Ok(new PerformanceAnalysisResult
        {
            SystemName = command.SystemName,
            Insights = insights,
            IsHealthy = insights.All(i => i.Severity < AlertSeverity.High)
        }));
    }
}

public sealed record PerformanceAnalysisCommand
{
    public required string SystemName { get; init; }
    public required decimal P99LatencyMs { get; init; }
    public required decimal LatencyBudgetMs { get; init; }
    public required decimal CurrentThroughput { get; init; }
    public required decimal ExpectedThroughput { get; init; }
    public required decimal CpuUtilization { get; init; }
    public required decimal MemoryUtilization { get; init; }
    public required string CorrelationId { get; init; }
}

public sealed record PerformanceInsight
{
    public required string Category { get; init; }
    public required string Metric { get; init; }
    public required decimal CurrentValue { get; init; }
    public required decimal Threshold { get; init; }
    public required AlertSeverity Severity { get; init; }
    public required string Recommendation { get; init; }
}

public sealed record PerformanceAnalysisResult
{
    public required string SystemName { get; init; }
    public required IReadOnlyList<PerformanceInsight> Insights { get; init; }
    public required bool IsHealthy { get; init; }
}
