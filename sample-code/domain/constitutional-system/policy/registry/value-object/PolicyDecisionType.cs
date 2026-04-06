namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

using Whycespace.Domain.SharedKernel;

public sealed class PolicyDecisionType : ValueObject
{
    public string Value { get; }

    private PolicyDecisionType(string value) => Value = value;

    public static readonly PolicyDecisionType Allow = new("ALLOW");
    public static readonly PolicyDecisionType Deny = new("DENY");
    public static readonly PolicyDecisionType Conditional = new("CONDITIONAL");

    private static readonly Dictionary<string, PolicyDecisionType> All = new()
    {
        [Allow.Value] = Allow,
        [Deny.Value] = Deny,
        [Conditional.Value] = Conditional
    };

    public static PolicyDecisionType From(string value)
    {
        if (!All.TryGetValue(value, out var decision))
            throw new ArgumentException($"Unknown policy decision type: {value}");
        return decision;
    }

    public bool IsPermissive => this == Allow || this == Conditional;
    public bool IsBlocking => this == Deny;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
