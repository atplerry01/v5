namespace Whycespace.Domain.BusinessSystem.Localization.RegionalRule;

public readonly record struct RegionalRuleId
{
    public Guid Value { get; }

    public RegionalRuleId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RegionalRuleId value must not be empty.", nameof(value));

        Value = value;
    }
}
