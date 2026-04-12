namespace Whycespace.Domain.BusinessSystem.Localization.RegionalRule;

public readonly record struct RuleCode
{
    public string Jurisdiction { get; }
    public string RuleType { get; }
    public string Identifier { get; }

    public RuleCode(string jurisdiction, string ruleType, string identifier)
    {
        if (string.IsNullOrWhiteSpace(jurisdiction))
            throw new ArgumentException("Jurisdiction must not be empty.", nameof(jurisdiction));

        if (string.IsNullOrWhiteSpace(ruleType))
            throw new ArgumentException("Rule type must not be empty.", nameof(ruleType));

        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Rule identifier must not be empty.", nameof(identifier));

        Jurisdiction = jurisdiction;
        RuleType = ruleType;
        Identifier = identifier;
    }
}
