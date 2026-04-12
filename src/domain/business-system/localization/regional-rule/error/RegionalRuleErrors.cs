namespace Whycespace.Domain.BusinessSystem.Localization.RegionalRule;

public static class RegionalRuleErrors
{
    public static RegionalRuleDomainException MissingId()
        => new("RegionalRuleId is required and must not be empty.");

    public static RegionalRuleDomainException InvalidRuleCode()
        => new("Regional rule must define jurisdiction, rule type, and identifier.");

    public static RegionalRuleDomainException InvalidStateTransition(RegionalRuleStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static RegionalRuleDomainException DuplicateRegionalRule(RuleCode code)
        => new($"Regional rule '{code.Jurisdiction}/{code.RuleType}/{code.Identifier}' already exists.");
}

public sealed class RegionalRuleDomainException : Exception
{
    public RegionalRuleDomainException(string message) : base(message) { }
}
