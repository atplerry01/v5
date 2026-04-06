using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T3I.Intelligence.Federation;

/// <summary>
/// T3I load distribution analyzer. Evaluates command distribution
/// across regions and suggests rebalancing. Stateless, deterministic.
/// </summary>
public sealed class LoadDistributionAnalyzer : IEngine<LoadDistributionCommand>
{
    public Task<EngineResult> ExecuteAsync(
        LoadDistributionCommand command, EngineContext context, CancellationToken cancellationToken = default)
    {
        var totalLoad = command.RegionLoads.Sum(r => r.CommandsPerSecond);
        if (totalLoad == 0)
            return Task.FromResult(EngineResult.Ok(new LoadDistributionResult
            {
                IsBalanced = true,
                Recommendations = [],
                ImbalanceScore = 0
            }));

        var avgLoad = totalLoad / command.RegionLoads.Count;
        var recommendations = new List<LoadRecommendation>();

        foreach (var region in command.RegionLoads)
        {
            var deviation = avgLoad > 0 ? (region.CommandsPerSecond - avgLoad) / avgLoad : 0;

            if (deviation > 0.3m) // 30% above average
            {
                recommendations.Add(new LoadRecommendation
                {
                    RegionId = region.RegionId,
                    Action = "ReduceLoad",
                    Description = $"Region '{region.RegionId}' is {deviation:P0} above average load",
                    SuggestedTargetLoad = avgLoad
                });
            }
            else if (deviation < -0.3m) // 30% below average
            {
                recommendations.Add(new LoadRecommendation
                {
                    RegionId = region.RegionId,
                    Action = "AcceptMoreLoad",
                    Description = $"Region '{region.RegionId}' is underutilized ({-deviation:P0} below average)",
                    SuggestedTargetLoad = avgLoad
                });
            }
        }

        var maxDeviation = command.RegionLoads.Max(r =>
            Math.Abs(avgLoad > 0 ? (r.CommandsPerSecond - avgLoad) / avgLoad : 0));

        return Task.FromResult(EngineResult.Ok(new LoadDistributionResult
        {
            IsBalanced = maxDeviation <= 0.3m,
            Recommendations = recommendations,
            ImbalanceScore = maxDeviation
        }));
    }
}

public sealed record LoadDistributionCommand
{
    public required IReadOnlyList<RegionLoad> RegionLoads { get; init; }
    public required string CorrelationId { get; init; }
}

public sealed record RegionLoad
{
    public required string RegionId { get; init; }
    public required decimal CommandsPerSecond { get; init; }
}

public sealed record LoadRecommendation
{
    public required string RegionId { get; init; }
    public required string Action { get; init; }
    public required string Description { get; init; }
    public required decimal SuggestedTargetLoad { get; init; }
}

public sealed record LoadDistributionResult
{
    public required bool IsBalanced { get; init; }
    public required IReadOnlyList<LoadRecommendation> Recommendations { get; init; }
    public required decimal ImbalanceScore { get; init; }
}
