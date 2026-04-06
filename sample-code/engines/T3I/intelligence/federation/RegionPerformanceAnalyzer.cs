using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.Intelligence.Federation;

/// <summary>
/// T3I region performance analyzer. Compares cross-region metrics
/// and identifies optimization opportunities. Stateless, deterministic.
/// </summary>
public sealed class RegionPerformanceAnalyzer : IEngine<RegionPerformanceAnalysisCommand>
{
    public Task<EngineResult> ExecuteAsync(
        RegionPerformanceAnalysisCommand command, EngineContext context, CancellationToken cancellationToken = default)
    {
        var insights = new List<RegionInsight>();

        foreach (var region in command.RegionMetrics)
        {
            // Latency outlier detection
            if (region.AvgLatencyMs > command.GlobalAvgLatencyMs * 1.5m)
            {
                insights.Add(new RegionInsight
                {
                    RegionId = region.RegionId,
                    Category = "LatencyOutlier",
                    Description = $"Region '{region.RegionId}' latency {region.AvgLatencyMs:F0}ms vs global avg {command.GlobalAvgLatencyMs:F0}ms",
                    Severity = region.AvgLatencyMs > command.GlobalAvgLatencyMs * 2m ? AlertSeverity.High : AlertSeverity.Medium,
                    Recommendation = "Review regional infrastructure and consider closer edge deployment"
                });
            }

            // Replication lag
            if (region.ReplicationLagMs > command.MaxAcceptableLagMs)
            {
                insights.Add(new RegionInsight
                {
                    RegionId = region.RegionId,
                    Category = "ReplicationLag",
                    Description = $"Region '{region.RegionId}' replication lag {region.ReplicationLagMs:F0}ms exceeds threshold {command.MaxAcceptableLagMs:F0}ms",
                    Severity = AlertSeverity.High,
                    Recommendation = "Review cross-region Kafka replication configuration"
                });
            }
        }

        return Task.FromResult(EngineResult.Ok(new RegionPerformanceAnalysisResult
        {
            Insights = insights,
            HasOutliers = insights.Any(i => i.Severity >= AlertSeverity.High)
        }));
    }
}

public sealed record RegionPerformanceAnalysisCommand
{
    public required IReadOnlyList<RegionMetricSnapshot> RegionMetrics { get; init; }
    public required decimal GlobalAvgLatencyMs { get; init; }
    public required decimal MaxAcceptableLagMs { get; init; }
    public required string CorrelationId { get; init; }
}

public sealed record RegionMetricSnapshot
{
    public required string RegionId { get; init; }
    public required decimal AvgLatencyMs { get; init; }
    public required decimal ReplicationLagMs { get; init; }
    public required decimal Throughput { get; init; }
}

public sealed record RegionInsight
{
    public required string RegionId { get; init; }
    public required string Category { get; init; }
    public required string Description { get; init; }
    public required AlertSeverity Severity { get; init; }
    public required string Recommendation { get; init; }
}

public sealed record RegionPerformanceAnalysisResult
{
    public required IReadOnlyList<RegionInsight> Insights { get; init; }
    public required bool HasOutliers { get; init; }
}
