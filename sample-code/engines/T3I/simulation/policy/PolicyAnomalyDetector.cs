using Whycespace.Engines.T3I.PolicySimulation.Scenario;

namespace Whycespace.Engines.T3I.PolicySimulation.Anomaly;

/// <summary>
/// Detects unusual outcomes in policy simulation results.
/// Identifies: contradictions, extreme deviations, workflow loops, coverage gaps.
/// Read-only — pure analysis, no side effects.
/// </summary>
public sealed class PolicyAnomalyDetector
{
    public IReadOnlyList<SimulationAnomaly> Detect(
        SimulationScenario scenario,
        DecisionSummary decisions,
        ImpactSummary? impact)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(decisions);

        var anomalies = new List<SimulationAnomaly>();

        DetectPolicyContradictions(decisions, anomalies);
        DetectExtremeRevenueDeviation(impact, anomalies);
        DetectCoverageGap(scenario, decisions, anomalies);
        DetectHighDenialRate(decisions, anomalies);
        DetectUnresolvedPolicies(decisions, anomalies);

        return anomalies.AsReadOnly();
    }

    private static void DetectPolicyContradictions(
        DecisionSummary decisions, List<SimulationAnomaly> anomalies)
    {
        var allowPolicies = decisions.PerPolicyDecisions.Where(d => d.Decision == "Allow").ToList();
        var denyPolicies = decisions.PerPolicyDecisions.Where(d => d.Decision == "Deny").ToList();

        if (allowPolicies.Count > 0 && denyPolicies.Count > 0)
        {
            anomalies.Add(new SimulationAnomaly(
                "PolicyContradiction",
                "HIGH",
                $"{allowPolicies.Count} policies allow and {denyPolicies.Count} policies deny the same action. " +
                "Priority resolution applies but the contradiction indicates misaligned policy intent.",
                "Review conflicting policies and consolidate rules to eliminate contradiction."));
        }
    }

    private static void DetectExtremeRevenueDeviation(
        ImpactSummary? impact, List<SimulationAnomaly> anomalies)
    {
        if (impact is null) return;

        if (impact.Economic.EstimatedRevenueEffect < -20)
        {
            anomalies.Add(new SimulationAnomaly(
                "ExtremeRevenueDeviation",
                "CRITICAL",
                $"Estimated revenue impact of {impact.Economic.EstimatedRevenueEffect:F1}% exceeds safe threshold.",
                "Consider phased rollout or narrower policy scope to limit economic impact."));
        }
    }

    private static void DetectCoverageGap(
        SimulationScenario scenario, DecisionSummary decisions, List<SimulationAnomaly> anomalies)
    {
        var unresolvedCount = decisions.PerPolicyDecisions.Count(d => d.Decision == "UNRESOLVED");

        if (unresolvedCount > 0 && unresolvedCount == scenario.Policies.Count)
        {
            anomalies.Add(new SimulationAnomaly(
                "CompleteCoverageGap",
                "CRITICAL",
                "No policy versions could be resolved — system would operate with zero policy coverage.",
                "Ensure at least one active policy version exists before activation."));
        }
        else if (unresolvedCount > 0)
        {
            anomalies.Add(new SimulationAnomaly(
                "PartialCoverageGap",
                "MEDIUM",
                $"{unresolvedCount} of {scenario.Policies.Count} policies could not be resolved.",
                "Verify missing policy versions are intentional."));
        }
    }

    private static void DetectHighDenialRate(
        DecisionSummary decisions, List<SimulationAnomaly> anomalies)
    {
        if (decisions.RulesEvaluated == 0) return;

        var failureRate = (double)decisions.RulesFailed / decisions.RulesEvaluated;
        if (failureRate > 0.8)
        {
            anomalies.Add(new SimulationAnomaly(
                "ExcessiveDenialRate",
                "HIGH",
                $"{failureRate:P0} of rules failed — policy may be overly restrictive.",
                "Review rule constraints for unintended over-restriction."));
        }
    }

    private static void DetectUnresolvedPolicies(
        DecisionSummary decisions, List<SimulationAnomaly> anomalies)
    {
        var unresolved = decisions.PerPolicyDecisions
            .Where(d => d.Decision == "UNRESOLVED")
            .ToList();

        foreach (var policy in unresolved)
        {
            var errorMsg = policy.Violations.FirstOrDefault() ?? "Unknown reason";
            anomalies.Add(new SimulationAnomaly(
                "UnresolvedPolicy",
                "MEDIUM",
                $"Policy {policy.PolicyId} could not be resolved: {errorMsg}",
                "Check that the specified policy version exists and is accessible."));
        }
    }
}
