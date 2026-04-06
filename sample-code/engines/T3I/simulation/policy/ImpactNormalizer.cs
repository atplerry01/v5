namespace Whycespace.Engines.T3I.PolicySimulation.Forecast;

/// <summary>
/// Normalizes impact outputs across economic, operational, and governance domains
/// into comparable 0–1 scores. Produces a composite score for cross-domain comparison.
/// </summary>
public static class ImpactNormalizer
{
    public static NormalizedImpactScores Normalize(ImpactSummary impact)
    {
        ArgumentNullException.ThrowIfNull(impact);

        var economicScore = NormalizeEconomic(impact.Economic);
        var operationalScore = NormalizeOperational(impact.Operational);
        var governanceScore = NormalizeGovernance(impact.Governance);

        // Weighted composite: economic (40%), operational (35%), governance (25%)
        var composite = (economicScore * 0.40) + (operationalScore * 0.35) + (governanceScore * 0.25);

        return new NormalizedImpactScores(
            Math.Round(economicScore, 4),
            Math.Round(operationalScore, 4),
            Math.Round(governanceScore, 4),
            Math.Round(composite, 4));
    }

    private static double NormalizeEconomic(EconomicImpact impact)
    {
        var severityScore = SeverityToScore(impact.Severity);

        // Revenue effect: clamped to [-100, 0] range, normalized to [0, 1]
        var revenueScore = Math.Clamp(Math.Abs(impact.EstimatedRevenueEffect) / 100.0, 0, 1);

        // Cost effect: clamped to [0, 100] range, normalized to [0, 1]
        var costScore = Math.Clamp(impact.EstimatedCostEffect / 100.0, 0, 1);

        return Math.Clamp((severityScore * 0.4) + (revenueScore * 0.35) + (costScore * 0.25), 0, 1);
    }

    private static double NormalizeOperational(OperationalImpact impact)
    {
        var severityScore = SeverityToScore(impact.Severity);

        // Blocked operations: each blocked op contributes to score, capped at 10
        var blockedScore = Math.Clamp(impact.BlockedOperations / 10.0, 0, 1);

        // Affected workflows: normalize against reasonable max of 20
        var workflowScore = Math.Clamp(impact.AffectedWorkflows / 20.0, 0, 1);

        return Math.Clamp((severityScore * 0.4) + (blockedScore * 0.35) + (workflowScore * 0.25), 0, 1);
    }

    private static double NormalizeGovernance(GovernanceImpact impact)
    {
        var severityScore = SeverityToScore(impact.Severity);

        // Approvals needed: normalize against max of 10
        var approvalScore = Math.Clamp(impact.AdditionalApprovalsRequired / 10.0, 0, 1);

        // Policy conflicts: each conflict is significant
        var conflictScore = Math.Clamp(impact.PolicyConflicts / 5.0, 0, 1);

        return Math.Clamp((severityScore * 0.4) + (approvalScore * 0.3) + (conflictScore * 0.3), 0, 1);
    }

    private static double SeverityToScore(string severity) =>
        severity.ToUpperInvariant() switch
        {
            "CRITICAL" => 1.0,
            "HIGH" => 0.75,
            "MEDIUM" => 0.5,
            "LOW" => 0.25,
            _ => 0.0
        };
}
