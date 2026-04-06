namespace Whycespace.Domain.ConstitutionalSystem.Policy.Enforcement;

using Whycespace.Domain.SharedKernel;

public sealed class EnforcementTargetType : ValueObject
{
    public string Value { get; }

    private EnforcementTargetType(string value) => Value = value;

    public static readonly EnforcementTargetType Identity = new("identity");
    public static readonly EnforcementTargetType Wallet = new("wallet");
    public static readonly EnforcementTargetType Spv = new("spv");
    public static readonly EnforcementTargetType Operator = new("operator");
    public static readonly EnforcementTargetType System = new("system");

    public static EnforcementTargetType From(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
