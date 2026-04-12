namespace Whycespace.Domain.ConstitutionalSystem.Policy.Enforcement;

public readonly record struct EnforcementId
{
    public Guid Value { get; }

    public EnforcementId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EnforcementId value must not be empty.", nameof(value));

        Value = value;
    }
}
