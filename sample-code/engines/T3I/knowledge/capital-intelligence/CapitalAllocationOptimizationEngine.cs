namespace Whycespace.Engines.T3I.CapitalIntelligence;

/// <summary>
/// T3I Capital Allocation Optimization Engine -- analyzes SPV capital allocation health.
///
/// Detects:
///   - Underutilized SPVs (utilization below 30%)
///   - Over-utilized SPVs (utilization above 90%)
///   - Concentration risk (single SPV holding over 40% of total capital)
///
/// Produces advisory recommendations only -- never mutates domain state.
/// Stateless. No persistence. Pure computation only.
/// No domain imports -- uses engine-local types only.
/// </summary>
public sealed class CapitalAllocationOptimizationEngine
{
    public AllocationOptimizationResult Optimize(OptimizeCapitalAllocationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var recommendations = new List<AllocationRecommendation>();

        // 1. Underutilized / over-utilized SPV detection
        foreach (var spv in command.SpvAllocations)
        {
            var utilization = spv.CurrentUtilization / Math.Max(spv.AllocatedCapital, 1m);

            if (utilization < 0.3m)
            {
                recommendations.Add(new AllocationRecommendation(
                    SpvId: spv.SpvId,
                    Action: RecommendationActions.ReduceAllocation,
                    Description: $"SPV utilization at {utilization:P0} -- consider reducing allocation",
                    SuggestedAdjustment: -(spv.AllocatedCapital * (1m - utilization) * 0.5m),
                    Priority: "High"));
            }
            else if (utilization > 0.9m)
            {
                recommendations.Add(new AllocationRecommendation(
                    SpvId: spv.SpvId,
                    Action: RecommendationActions.IncreaseAllocation,
                    Description: $"SPV utilization at {utilization:P0} -- at risk of capital constraint",
                    SuggestedAdjustment: spv.AllocatedCapital * 0.2m,
                    Priority: "Medium"));
            }
        }

        // 2. Concentration risk
        var totalCapital = command.SpvAllocations.Sum(s => s.AllocatedCapital);
        if (totalCapital > 0)
        {
            foreach (var spv in command.SpvAllocations)
            {
                var concentration = spv.AllocatedCapital / totalCapital;
                if (concentration > 0.4m)
                {
                    recommendations.Add(new AllocationRecommendation(
                        SpvId: spv.SpvId,
                        Action: RecommendationActions.Diversify,
                        Description: $"SPV holds {concentration:P0} of total capital -- concentration risk",
                        SuggestedAdjustment: 0m,
                        Priority: "High"));
                }
            }
        }

        var overallScore = recommendations.Count == 0
            ? 1.0m
            : Math.Max(0m, 1.0m - (recommendations.Count * 0.15m));

        return new AllocationOptimizationResult(recommendations, overallScore);
    }

    /// <summary>
    /// Engine-local recommendation action constants.
    /// </summary>
    private static class RecommendationActions
    {
        public const string ReduceAllocation = "REDUCE_ALLOCATION";
        public const string IncreaseAllocation = "INCREASE_ALLOCATION";
        public const string Diversify = "DIVERSIFY";
    }
}

// -- Commands --

public sealed record OptimizeCapitalAllocationCommand(
    IReadOnlyList<SpvAllocationSnapshot> SpvAllocations);

public sealed record SpvAllocationSnapshot(
    Guid SpvId,
    decimal AllocatedCapital,
    decimal CurrentUtilization);

// -- Results --

public sealed record AllocationOptimizationResult(
    IReadOnlyList<AllocationRecommendation> Recommendations,
    decimal OverallHealthScore);

public sealed record AllocationRecommendation(
    Guid SpvId,
    string Action,
    string Description,
    decimal SuggestedAdjustment,
    string Priority);
