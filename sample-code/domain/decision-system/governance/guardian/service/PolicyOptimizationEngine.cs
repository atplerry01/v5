namespace Whycespace.Domain.DecisionSystem.Governance.Guardian;

/// <summary>
/// Domain service that finds optimal policy combinations given constraints.
/// Pure domain logic — deterministic, no side effects.
/// </summary>
public sealed class PolicyOptimizationEngine
{
    /// <summary>
    /// Given a set of policy evaluation scores, identifies the optimal subset
    /// that maximizes composite impact while respecting risk constraints.
    /// </summary>
    public OptimizationResult Optimize(IReadOnlyList<PolicyCandidate> candidates, int maxRiskThreshold)
    {
        if (candidates.Count == 0)
            return OptimizationResult.Empty;

        var sorted = candidates
            .Where(c => c.RiskScore <= maxRiskThreshold)
            .OrderByDescending(c => c.CompositeImpact)
            .ToList();

        var selected = new List<PolicyCandidate>();
        var totalRisk = 0;

        foreach (var candidate in sorted)
        {
            if (totalRisk + candidate.RiskScore <= maxRiskThreshold)
            {
                selected.Add(candidate);
                totalRisk += candidate.RiskScore;
            }
        }

        var avgImpact = selected.Count > 0
            ? selected.Average(c => c.CompositeImpact)
            : 0.0;

        return new OptimizationResult(selected, avgImpact, totalRisk);
    }
}

public sealed record PolicyCandidate(
    Guid PolicyId,
    double CompositeImpact,
    int RiskScore,
    string Rationale);

public sealed record OptimizationResult(
    IReadOnlyList<PolicyCandidate> SelectedPolicies,
    double AverageImpact,
    int TotalRisk)
{
    public static OptimizationResult Empty => new([], 0.0, 0);
}
