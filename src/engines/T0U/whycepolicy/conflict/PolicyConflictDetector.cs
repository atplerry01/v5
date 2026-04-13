using System.Security.Cryptography;
using System.Text;
using Whycespace.Engines.T0U.WhycePolicy.Registry;

namespace Whycespace.Engines.T0U.WhycePolicy.Conflict;

/// <summary>
/// Detects conflicts between policy rules.
/// Two rules conflict when they produce contradictory decisions for the same input.
/// </summary>
public static class PolicyConflictDetector
{
    public static PolicyConflict[] DetectConflicts(IReadOnlyList<PolicyRule> rules)
    {
        var conflicts = new List<PolicyConflict>();

        for (var i = 0; i < rules.Count; i++)
        {
            for (var j = i + 1; j < rules.Count; j++)
            {
                if (string.Equals(rules[i].PolicyName, rules[j].PolicyName, StringComparison.Ordinal) &&
                    rules[i].Priority == rules[j].Priority)
                {
                    var conflictHash = ComputeConflictHash(rules[i].RuleId, rules[j].RuleId);
                    conflicts.Add(new PolicyConflict(
                        rules[i].RuleId, rules[j].RuleId,
                        "Same policy and priority — deterministic ordering cannot be guaranteed.",
                        conflictHash));
                }
            }
        }

        return conflicts.ToArray();
    }

    private static string ComputeConflictHash(string ruleIdA, string ruleIdB)
    {
        var ordered = StringComparer.Ordinal.Compare(ruleIdA, ruleIdB) <= 0
            ? $"{ruleIdA}:{ruleIdB}"
            : $"{ruleIdB}:{ruleIdA}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"conflict:{ordered}"));
        return Convert.ToHexStringLower(bytes);
    }
}

public sealed record PolicyConflict(
    string RuleIdA,
    string RuleIdB,
    string Reason,
    string ConflictHash);
