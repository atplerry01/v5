namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public readonly record struct RuleDefinition
{
    public string RuleName { get; }
    public string PolicyReference { get; }

    public RuleDefinition(string ruleName, string policyReference)
    {
        if (string.IsNullOrWhiteSpace(ruleName))
            throw new ArgumentException("RuleName must not be empty.", nameof(ruleName));

        if (string.IsNullOrWhiteSpace(policyReference))
            throw new ArgumentException("PolicyReference must not be empty.", nameof(policyReference));

        RuleName = ruleName;
        PolicyReference = policyReference;
    }
}
