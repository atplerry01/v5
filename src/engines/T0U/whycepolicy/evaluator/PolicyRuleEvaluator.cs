using System.Security.Cryptography;
using System.Text;
using Whyce.Engines.T0U.WhycePolicy.Registry;

namespace Whyce.Engines.T0U.WhycePolicy.Evaluator;

/// <summary>
/// Evaluates policy rules against a command execution context.
/// Deterministic: same inputs always produce same evaluation.
/// </summary>
public static class PolicyRuleEvaluator
{
    public static (bool IsCompliant, string[] EvaluatedRules, string? DenialReason) Evaluate(
        IReadOnlyList<PolicyRule> rules,
        string identityId,
        string[] roles,
        int trustScore,
        string commandType)
    {
        var evaluatedRules = new List<string>(rules.Count);

        foreach (var rule in rules)
        {
            evaluatedRules.Add(rule.RuleName);

            if (!rule.Evaluate(identityId, roles, trustScore, commandType))
            {
                return (false, evaluatedRules.ToArray(),
                    $"Policy rule '{rule.RuleName}' denied execution.");
            }
        }

        return (true, evaluatedRules.ToArray(), null);
    }

    public static string ComputeEvaluationHash(
        string policyName, string identityId, string[] roles, int trustScore, bool isCompliant)
    {
        var sortedRoles = string.Join(",", roles.OrderBy(r => r, StringComparer.Ordinal));
        var input = $"{policyName}:{identityId}:{sortedRoles}:{trustScore}:{isCompliant}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }
}
