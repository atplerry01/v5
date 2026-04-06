using Whycespace.Engines.T3I.PolicySimulation.Scenario;
using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T3I.PolicySimulation.Evaluator;

/// <summary>
/// Executes policy evaluation in simulation mode.
/// Calls the shared IPolicyEvaluator contract — NO enforcement, NO state mutation.
/// Collects decision output only.
/// </summary>
public sealed class PolicySimulationEvaluator
{
    private readonly IPolicyEvaluator _evaluator;

    public PolicySimulationEvaluator(IPolicyEvaluator evaluator)
    {
        _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
    }

    public async Task<DecisionSummary> EvaluateAsync(
        SimulationScenario scenario,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        var perPolicyDecisions = new List<SimulatedDecision>();
        var allViolations = new List<string>();
        var totalRules = 0;
        var passedRules = 0;
        var failedRules = 0;

        foreach (var policy in scenario.Policies)
        {
            if (!policy.IsResolved)
            {
                perPolicyDecisions.Add(new SimulatedDecision(
                    policy.PolicyId, "UNRESOLVED", [policy.Error ?? "Version not found"]));
                continue;
            }

            var input = new PolicyEvaluationInput(
                policy.PolicyId,
                scenario.Context.ActorId,
                scenario.Context.Action,
                scenario.Context.Resource,
                scenario.Context.Environment ?? "simulation",
                scenario.SimulatedTime);

            var result = await _evaluator.EvaluateAsync(input, cancellationToken);

            var decision = result.DecisionType;
            var violations = result.Violations?.ToList() ?? [];

            perPolicyDecisions.Add(new SimulatedDecision(policy.PolicyId, decision, violations));
            allViolations.AddRange(violations);

            if (result.EvaluatedRules is not null)
            {
                totalRules += result.EvaluatedRules.Count;
                passedRules += result.EvaluatedRules.Count(r => r.Passed);
                failedRules += result.EvaluatedRules.Count(r => !r.Passed);
            }
        }

        var overallDecision = DetermineOverallDecision(perPolicyDecisions);

        return new DecisionSummary(
            overallDecision,
            perPolicyDecisions.Count,
            totalRules,
            passedRules,
            failedRules,
            allViolations.AsReadOnly(),
            perPolicyDecisions.AsReadOnly());
    }

    private static string DetermineOverallDecision(IReadOnlyList<SimulatedDecision> decisions)
    {
        if (decisions.Any(d => d.Decision == "Deny"))
            return "Deny";
        if (decisions.Any(d => d.Decision == "Conditional"))
            return "Conditional";
        if (decisions.Any(d => d.Decision == "UNRESOLVED"))
            return "Inconclusive";
        return "Allow";
    }
}
