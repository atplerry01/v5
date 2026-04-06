using Whycespace.Engines.T3I.PolicySimulation.Scenario;

namespace Whycespace.Engines.T3I.PolicySimulation.Risk;

/// <summary>
/// Assigns risk scores (0–100) to policy simulation scenarios.
/// Evaluates: policy conflicts, economic disruption, governance overload, anomaly likelihood.
/// Read-only — pure computation, no side effects.
/// </summary>
public sealed class PolicyRiskEngine
{
    public RiskScore Evaluate(
        SimulationScenario scenario,
        DecisionSummary decisions,
        ImpactSummary? impact)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(decisions);

        var factors = new List<RiskFactor>();
        var totalScore = 0;

        // Factor 1: Policy conflict risk
        var conflictRisk = EvaluateConflictRisk(decisions);
        factors.Add(conflictRisk);
        totalScore += conflictRisk.Weight;

        // Factor 2: Economic disruption risk
        var economicRisk = EvaluateEconomicRisk(impact);
        factors.Add(economicRisk);
        totalScore += economicRisk.Weight;

        // Factor 3: Governance overload risk
        var governanceRisk = EvaluateGovernanceRisk(scenario);
        factors.Add(governanceRisk);
        totalScore += governanceRisk.Weight;

        // Factor 4: Unresolved policy risk
        var unresolvedRisk = EvaluateUnresolvedRisk(decisions);
        factors.Add(unresolvedRisk);
        totalScore += unresolvedRisk.Weight;

        // Factor 5: Violation density
        var violationRisk = EvaluateViolationDensity(decisions);
        factors.Add(violationRisk);
        totalScore += violationRisk.Weight;

        var clampedScore = Math.Clamp(totalScore, 0, 100);
        var category = CategorizeRisk(clampedScore);

        return new RiskScore(clampedScore, category, factors.AsReadOnly());
    }

    private static RiskFactor EvaluateConflictRisk(DecisionSummary decisions)
    {
        var hasAllow = decisions.PerPolicyDecisions.Any(d => d.Decision == "Allow");
        var hasDeny = decisions.PerPolicyDecisions.Any(d => d.Decision == "Deny");

        if (hasAllow && hasDeny)
            return new RiskFactor("PolicyConflict", 30, "Conflicting allow/deny decisions detected among evaluated policies.");

        return new RiskFactor("PolicyConflict", 0, "No policy conflicts detected.");
    }

    private static RiskFactor EvaluateEconomicRisk(ImpactSummary? impact)
    {
        if (impact is null)
            return new RiskFactor("EconomicDisruption", 5, "Impact analysis not available — minimal risk assumed.");

        return impact.Economic.Severity switch
        {
            "HIGH" => new RiskFactor("EconomicDisruption", 25, impact.Economic.Explanation),
            "MEDIUM" => new RiskFactor("EconomicDisruption", 15, impact.Economic.Explanation),
            _ => new RiskFactor("EconomicDisruption", 0, "No economic disruption expected.")
        };
    }

    private static RiskFactor EvaluateGovernanceRisk(SimulationScenario scenario)
    {
        var policyCount = scenario.Policies.Count;
        if (policyCount > 5)
            return new RiskFactor("GovernanceOverload", 20, $"{policyCount} policies require governance — high approval burden.");
        if (policyCount > 2)
            return new RiskFactor("GovernanceOverload", 10, $"{policyCount} policies require governance review.");

        return new RiskFactor("GovernanceOverload", 0, "Governance load is manageable.");
    }

    private static RiskFactor EvaluateUnresolvedRisk(DecisionSummary decisions)
    {
        var unresolved = decisions.PerPolicyDecisions.Count(d => d.Decision == "UNRESOLVED");
        if (unresolved > 0)
            return new RiskFactor("UnresolvedPolicies", 15, $"{unresolved} policy version(s) could not be resolved.");

        return new RiskFactor("UnresolvedPolicies", 0, "All policy versions resolved successfully.");
    }

    private static RiskFactor EvaluateViolationDensity(DecisionSummary decisions)
    {
        if (decisions.RulesEvaluated == 0)
            return new RiskFactor("ViolationDensity", 5, "No rules evaluated — insufficient data.");

        var failureRate = (double)decisions.RulesFailed / decisions.RulesEvaluated;
        if (failureRate > 0.5)
            return new RiskFactor("ViolationDensity", 20, $"{failureRate:P0} rule failure rate — high violation density.");
        if (failureRate > 0.2)
            return new RiskFactor("ViolationDensity", 10, $"{failureRate:P0} rule failure rate — moderate violation density.");

        return new RiskFactor("ViolationDensity", 0, "Violation density is within normal range.");
    }

    private static string CategorizeRisk(int score) => score switch
    {
        >= 70 => "CRITICAL",
        >= 40 => "HIGH",
        >= 20 => "MEDIUM",
        _ => "LOW"
    };
}
