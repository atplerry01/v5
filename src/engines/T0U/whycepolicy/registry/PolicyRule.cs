using System.Security.Cryptography;
using System.Text;

namespace Whycespace.Engines.T0U.WhycePolicy.Registry;

/// <summary>
/// Immutable policy rule. Rules are evaluated deterministically.
/// </summary>
public sealed record PolicyRule(
    string RuleId,
    string RuleName,
    string PolicyName,
    int Priority,
    string RuleHash)
{
    /// <summary>
    /// Evaluates whether this rule allows execution given the context.
    /// Must be deterministic — same inputs always produce same result.
    /// </summary>
    public bool Evaluate(string identityId, string[] roles, int trustScore, string commandType)
    {
        // Default rules: identity must exist, trust score must meet minimum
        if (string.IsNullOrEmpty(identityId))
            return false;

        if (trustScore < 10)
            return false;

        return true;
    }

    public static string ComputeRuleHash(string ruleId, string ruleName, string policyName)
    {
        var input = $"rule:{ruleId}:{ruleName}:{policyName}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }
}
