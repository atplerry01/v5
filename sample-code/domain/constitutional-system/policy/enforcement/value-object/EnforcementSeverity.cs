namespace Whycespace.Domain.ConstitutionalSystem.Policy.Enforcement;

using Whycespace.Domain.SharedKernel;

public sealed class EnforcementSeverity : ValueObject
{
    public string Value { get; }

    private EnforcementSeverity(string value) => Value = value;

    public static readonly EnforcementSeverity Soft = new("soft");
    public static readonly EnforcementSeverity Medium = new("medium");
    public static readonly EnforcementSeverity Hard = new("hard");
    public static readonly EnforcementSeverity Critical = new("critical");

    private static readonly Dictionary<string, EnforcementSeverity> All = new()
    {
        [Soft.Value] = Soft,
        [Medium.Value] = Medium,
        [Hard.Value] = Hard,
        [Critical.Value] = Critical
    };

    public static EnforcementSeverity From(string value)
    {
        if (!All.TryGetValue(value, out var severity))
            throw new ArgumentException($"Unknown enforcement severity: {value}");
        return severity;
    }

    public bool IsHigherThan(EnforcementSeverity other) => Rank(this) > Rank(other);

    private static int Rank(EnforcementSeverity s) => s.Value switch
    {
        "soft" => 0, "medium" => 1, "hard" => 2, "critical" => 3, _ => -1
    };

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
