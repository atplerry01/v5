namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public readonly record struct RuleId
{
    public Guid Value { get; }

    public RuleId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RuleId cannot be empty.", nameof(value));

        Value = value;
    }
}
