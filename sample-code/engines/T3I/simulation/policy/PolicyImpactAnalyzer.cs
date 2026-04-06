using Whycespace.Engines.T3I.PolicySimulation.Scenario;

namespace Whycespace.Engines.T3I.PolicySimulation.Forecast;

/// <summary>
/// Evaluates downstream effects of policy activation.
/// Read-only — uses projection data and simulation results.
/// Produces economic, operational, and governance impact assessments.
/// </summary>
public sealed class PolicyImpactAnalyzer
{
    public ImpactSummary Analyze(SimulationScenario scenario, DecisionSummary decisions)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(decisions);

        var economic = AnalyzeEconomicImpact(decisions);
        var operational = AnalyzeOperationalImpact(decisions);
        var governance = AnalyzeGovernanceImpact(scenario, decisions);

        return new ImpactSummary(economic, operational, governance);
    }

    private static EconomicImpact AnalyzeEconomicImpact(DecisionSummary decisions)
    {
        // Estimate based on deny/violation patterns
        var denyCount = decisions.PerPolicyDecisions.Count(d => d.Decision == "Deny");
        var violationCount = decisions.Violations.Count;

        if (denyCount == 0 && violationCount == 0)
        {
            return new EconomicImpact("LOW", 0, 0,
                "No economic disruption expected — all policies pass.");
        }

        var severity = denyCount > 2 ? "HIGH" : denyCount > 0 ? "MEDIUM" : "LOW";
        var revenueEffect = -denyCount * 5.0; // Simplified: each deny blocks ~5% revenue flow
        var costEffect = violationCount * 1.0;  // Each violation adds ~1% compliance cost

        return new EconomicImpact(severity, revenueEffect, costEffect,
            $"{denyCount} policy denials may block revenue operations. {violationCount} violations require remediation.");
    }

    private static OperationalImpact AnalyzeOperationalImpact(DecisionSummary decisions)
    {
        var blockedOps = decisions.PerPolicyDecisions.Count(d => d.Decision == "Deny");
        var affectedWorkflows = decisions.PoliciesEvaluated;

        var severity = blockedOps > 3 ? "HIGH" : blockedOps > 0 ? "MEDIUM" : "LOW";

        return new OperationalImpact(severity, affectedWorkflows, blockedOps,
            blockedOps > 0
                ? $"{blockedOps} operations would be blocked under this policy configuration."
                : "No operational disruption expected.");
    }

    private static GovernanceImpact AnalyzeGovernanceImpact(
        SimulationScenario scenario, DecisionSummary decisions)
    {
        var policyCount = scenario.Policies.Count;
        var conflictCount = CountConflicts(decisions);
        var approvalsNeeded = policyCount; // Each policy change needs governance approval

        var severity = conflictCount > 0 ? "HIGH" : policyCount > 3 ? "MEDIUM" : "LOW";

        return new GovernanceImpact(severity, approvalsNeeded, conflictCount,
            conflictCount > 0
                ? $"{conflictCount} policy conflicts detected — requires governance review."
                : $"{approvalsNeeded} approval(s) required for activation.");
    }

    private static int CountConflicts(DecisionSummary decisions)
    {
        // Detect conflicting decisions among policies at the same level
        var hasAllow = decisions.PerPolicyDecisions.Any(d => d.Decision == "Allow");
        var hasDeny = decisions.PerPolicyDecisions.Any(d => d.Decision == "Deny");
        return hasAllow && hasDeny ? 1 : 0;
    }
}
