using System.Security.Cryptography;
using System.Text;
using Whyce.Engines.T0U.WhycePolicy.Command;
using Whyce.Engines.T0U.WhycePolicy.Context;
using Whyce.Engines.T0U.WhycePolicy.Evaluator;
using Whyce.Engines.T0U.WhycePolicy.Registry;
using Whyce.Engines.T0U.WhycePolicy.Result;
using Whyce.Engines.T0U.WhycePolicy.Safeguard;
using Whyce.Engines.T0U.WhycePolicy.Simulation;

namespace Whyce.Engines.T0U.WhycePolicy.Engine;

/// <summary>
/// WhycePolicy — Constitutional Policy Engine (T0U).
/// Non-bypassable. Every command MUST pass policy evaluation.
///
/// If IsCompliant == false → STOP execution immediately.
///
/// Capabilities:
/// - Evaluate: Full policy evaluation against command context
/// - Simulate: Dry-run policy evaluation for impact assessment
/// </summary>
public sealed class WhycePolicyEngine
{
    private const string PolicyVersion = "1.0.0";
    private readonly PolicyRegistry _registry;
    private readonly IReadOnlyList<PolicySafeguard> _safeguards;

    public WhycePolicyEngine(PolicyRegistry registry, IReadOnlyList<PolicySafeguard> safeguards)
    {
        _registry = registry;
        _safeguards = safeguards;
    }

    /// <summary>
    /// Creates engine with default constitutional rules.
    /// </summary>
    public WhycePolicyEngine() : this(
        PolicyRegistry.CreateDefault(),
        CreateDefaultSafeguards())
    {
    }

    /// <summary>
    /// Evaluates policy for a command execution.
    /// Returns PolicyEvaluationResult with compliance decision.
    /// Non-compliant = execution MUST be halted.
    /// </summary>
    public Task<PolicyEvaluationResult> Evaluate(EvaluatePolicyCommand command)
    {
        // Step 1: Evaluate constitutional safeguards first
        foreach (var safeguard in _safeguards)
        {
            if (!safeguard.Evaluate(command.IdentityId, command.TrustScore))
            {
                var denialHash = ComputeDecisionHash(
                    command.PolicyName, command.IdentityId, command.Roles,
                    command.TrustScore, isCompliant: false);

                return Task.FromResult(new PolicyEvaluationResult(
                    IsCompliant: false,
                    DecisionHash: denialHash,
                    ExecutionHash: string.Empty,
                    PolicyVersion: PolicyVersion,
                    RulesEvaluated: [safeguard.SafeguardName],
                    DenialReason: $"Constitutional safeguard '{safeguard.SafeguardName}' violated."));
            }
        }

        // Step 2: Resolve policy rules
        var rules = _registry.GetRulesForPolicy(command.PolicyName);
        if (rules.Count == 0)
        {
            rules = _registry.GetRulesForPolicy("default");
        }

        // Step 3: Evaluate all rules
        var (isCompliant, evaluatedRules, denialReason) = PolicyRuleEvaluator.Evaluate(
            rules, command.IdentityId, command.Roles, command.TrustScore, command.CommandType);

        // Step 4: Compute deterministic decision hash
        var decisionHash = ComputeDecisionHash(
            command.PolicyName, command.IdentityId, command.Roles,
            command.TrustScore, isCompliant);

        var executionHash = PolicyRuleEvaluator.ComputeEvaluationHash(
            command.PolicyName, command.IdentityId, command.Roles,
            command.TrustScore, isCompliant);

        return Task.FromResult(new PolicyEvaluationResult(
            IsCompliant: isCompliant,
            DecisionHash: decisionHash,
            ExecutionHash: executionHash,
            PolicyVersion: PolicyVersion,
            RulesEvaluated: evaluatedRules,
            DenialReason: denialReason));
    }

    /// <summary>
    /// Simulates policy evaluation without enforcement.
    /// </summary>
    public Task<PolicySimulationResult> Simulate(EvaluatePolicyCommand command)
    {
        var rules = _registry.GetRulesForPolicy(command.PolicyName);
        if (rules.Count == 0)
            rules = _registry.GetRulesForPolicy("default");

        var (wouldBeCompliant, evaluatedRules, denialReason) = PolicyRuleEvaluator.Evaluate(
            rules, command.IdentityId, command.Roles, command.TrustScore, command.CommandType);

        var simulationHash = ComputeDecisionHash(
            $"sim:{command.PolicyName}", command.IdentityId, command.Roles,
            command.TrustScore, wouldBeCompliant);

        return Task.FromResult(new PolicySimulationResult(
            WouldBeCompliant: wouldBeCompliant,
            RulesEvaluated: evaluatedRules,
            SimulationHash: simulationHash,
            PredictedDenialReason: denialReason));
    }

    private static string ComputeDecisionHash(
        string policyName, string identityId, string[] roles, int trustScore, bool isCompliant)
    {
        var sortedRoles = string.Join(",", roles.OrderBy(r => r, StringComparer.Ordinal));
        // PolicyVersion included in hash — replay with different version produces different hash
        var input = $"{PolicyVersion}:{policyName}:{identityId}:{sortedRoles}:{trustScore}:{isCompliant}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }

    private static IReadOnlyList<PolicySafeguard> CreateDefaultSafeguards()
    {
        return
        [
            new PolicySafeguard(
                "safeguard-identity", "IdentityRequired",
                "Identity must be resolved before policy evaluation.",
                ComputeSafeguardHash("safeguard-identity")),
            new PolicySafeguard(
                "safeguard-trust-floor", "TrustFloor",
                "Trust score must be non-negative.",
                ComputeSafeguardHash("safeguard-trust-floor"))
        ];
    }

    private static string ComputeSafeguardHash(string safeguardId)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"safeguard:{safeguardId}"));
        return Convert.ToHexStringLower(bytes);
    }
}
