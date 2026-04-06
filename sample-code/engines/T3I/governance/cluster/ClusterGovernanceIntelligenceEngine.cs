using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T3I.Governance.Cluster;

/// <summary>
/// E18.7.3 — Intelligence signal engine for cluster governance.
/// Provides risk assessment and recommendations.
///
/// CRITICAL: Intelligence RECOMMENDS but NEVER overrides policy.
/// Signals are advisory — final authority is always WHYCEPOLICY.
/// Stateless, deterministic. NEVER writes to chain or mutates state.
/// </summary>
public sealed class ClusterGovernanceIntelligenceEngine
{
    public Task<GovernanceSignal> AnalyzeAsync(
        ClusterGovernanceInput input,
        CancellationToken ct = default)
    {
        var riskScore = ComputeRiskScore(input);
        var recommendation = riskScore switch
        {
            > 0.8m => "DENY",
            > 0.5m => "REVIEW",
            _ => "APPROVE"
        };

        return Task.FromResult(new GovernanceSignal
        {
            RiskScore = riskScore,
            Recommendation = recommendation,
            ClusterId = input.ClusterId,
            DecisionType = input.DecisionType
        });
    }

    private static decimal ComputeRiskScore(ClusterGovernanceInput input)
    {
        // Risk increases with economic impact
        var baseRisk = Math.Min(input.EconomicImpact / 1_000_000m, 1.0m);
        return Math.Round(baseRisk * 0.3m, 2);
    }
}

public sealed record GovernanceSignal
{
    public decimal RiskScore { get; init; }
    public string Recommendation { get; init; } = string.Empty;
    public Guid ClusterId { get; init; }
    public string DecisionType { get; init; } = string.Empty;
}
