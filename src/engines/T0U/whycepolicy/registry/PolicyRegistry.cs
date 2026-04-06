namespace Whyce.Engines.T0U.WhycePolicy.Registry;

/// <summary>
/// Registry of all policy rules. Provides deterministic rule resolution.
/// Rules are loaded once and immutable at runtime.
/// </summary>
public sealed class PolicyRegistry
{
    private readonly IReadOnlyList<PolicyRule> _rules;

    public PolicyRegistry(IReadOnlyList<PolicyRule> rules)
    {
        _rules = rules;
    }

    public IReadOnlyList<PolicyRule> GetRulesForPolicy(string policyName)
    {
        return _rules
            .Where(r => string.Equals(r.PolicyName, policyName, StringComparison.Ordinal))
            .OrderBy(r => r.Priority)
            .ToList();
    }

    public IReadOnlyList<PolicyRule> GetAllRules() => _rules;

    /// <summary>
    /// Creates a default registry with baseline constitutional rules.
    /// </summary>
    public static PolicyRegistry CreateDefault()
    {
        var rules = new List<PolicyRule>
        {
            new("rule-identity-required", "IdentityRequired", "default",
                Priority: 0,
                RuleHash: PolicyRule.ComputeRuleHash("rule-identity-required", "IdentityRequired", "default")),
            new("rule-trust-minimum", "TrustMinimum", "default",
                Priority: 1,
                RuleHash: PolicyRule.ComputeRuleHash("rule-trust-minimum", "TrustMinimum", "default"))
        };

        return new PolicyRegistry(rules);
    }
}
