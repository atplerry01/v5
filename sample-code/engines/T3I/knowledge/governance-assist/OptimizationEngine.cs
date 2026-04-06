using Whycespace.Engines.T3I.PolicySimulation;

namespace Whycespace.Engines.T3I.GovernanceAssist;

/// <summary>
/// Maximizes governance outcomes under policy constraints.
/// Consumes simulation results and delegates to domain optimization service.
/// Deterministic, read-only, no side effects.
/// </summary>
public sealed class OptimizationEngine
{
    private readonly PolicyOptimizationEngine _domainOptimizer;

    public OptimizationEngine(PolicyOptimizationEngine domainOptimizer)
    {
        _domainOptimizer = domainOptimizer;
    }

    public OptimizationSummary? Optimize(
        IReadOnlyList<PolicySimulationResult> simulationResults,
        int maxRiskThreshold)
    {
        var candidates = simulationResults
            .SelectMany(r => r.PolicyVersionsUsed.Select(pv => new
            {
                pv.PolicyId,
                Impact = r.NormalizedImpact?.CompositeScore ?? 0.0,
                Risk = r.RiskScore?.Score ?? 0,
                Decision = r.DecisionSummary.OverallDecision
            }))
            .GroupBy(x => x.PolicyId)
            .Select(g => new PolicyCandidate(
                g.Key,
                g.Average(x => x.Impact),
                (int)g.Average(x => x.Risk),
                $"Based on {g.Count()} simulation(s), overall decision: {g.Last().Decision}"))
            .ToList();

        if (candidates.Count == 0)
            return null;

        var result = _domainOptimizer.Optimize(candidates, maxRiskThreshold);

        return new OptimizationSummary
        {
            SelectedPolicies = result.SelectedPolicies.Select(p => p.PolicyId).ToList(),
            AverageImpact = result.AverageImpact,
            TotalRisk = result.TotalRisk
        };
    }
}
