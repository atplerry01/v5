using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.Intelligence.Economic;

/// <summary>
/// T3I revenue optimization analyzer. Evaluates revenue streams
/// and identifies optimization opportunities. Stateless, deterministic.
/// </summary>
public sealed class RevenueOptimizationAnalyzer : IEngine<RevenueOptimizationCommand>
{
    public Task<EngineResult> ExecuteAsync(
        RevenueOptimizationCommand command, EngineContext context, CancellationToken cancellationToken = default)
    {
        var opportunities = new List<OptimizationOpportunity>();

        // Detect underperforming revenue streams
        if (command.ActualRevenue < command.ProjectedRevenue * 0.8m)
        {
            var gap = command.ProjectedRevenue - command.ActualRevenue;
            opportunities.Add(new OptimizationOpportunity
            {
                Category = "RevenueGap",
                Description = $"Revenue shortfall of {gap:C0} ({gap / command.ProjectedRevenue:P0} below projection)",
                EstimatedImpact = gap,
                Confidence = ConfidenceScore.High,
                Severity = gap > command.ProjectedRevenue * 0.3m ? AlertSeverity.Critical : AlertSeverity.High
            });
        }

        // Detect pricing inefficiency
        if (command.AverageMargin < command.TargetMargin * 0.7m)
        {
            opportunities.Add(new OptimizationOpportunity
            {
                Category = "PricingInefficiency",
                Description = $"Average margin {command.AverageMargin:P1} below target {command.TargetMargin:P1}",
                EstimatedImpact = command.ActualRevenue * (command.TargetMargin - command.AverageMargin),
                Confidence = ConfidenceScore.Medium,
                Severity = AlertSeverity.Medium
            });
        }

        return Task.FromResult(EngineResult.Ok(new RevenueOptimizationResult
        {
            Opportunities = opportunities,
            TotalEstimatedImpact = opportunities.Sum(o => o.EstimatedImpact)
        }));
    }
}

public sealed record RevenueOptimizationCommand
{
    public required decimal ActualRevenue { get; init; }
    public required decimal ProjectedRevenue { get; init; }
    public required decimal AverageMargin { get; init; }
    public required decimal TargetMargin { get; init; }
    public required string CorrelationId { get; init; }
}

public sealed record OptimizationOpportunity
{
    public required string Category { get; init; }
    public required string Description { get; init; }
    public required decimal EstimatedImpact { get; init; }
    public required ConfidenceScore Confidence { get; init; }
    public required AlertSeverity Severity { get; init; }
}

public sealed record RevenueOptimizationResult
{
    public required IReadOnlyList<OptimizationOpportunity> Opportunities { get; init; }
    public required decimal TotalEstimatedImpact { get; init; }
}
