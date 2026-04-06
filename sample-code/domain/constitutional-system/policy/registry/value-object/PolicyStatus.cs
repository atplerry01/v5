namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

using Whycespace.Domain.SharedKernel;

public sealed class PolicyStatus : ValueObject
{
    public string Value { get; }

    private PolicyStatus(string value) => Value = value;

    public static readonly PolicyStatus Draft = new("draft");
    public static readonly PolicyStatus Active = new("active");
    public static readonly PolicyStatus Suspended = new("suspended");
    public static readonly PolicyStatus Archived = new("archived");

    private static readonly Dictionary<string, PolicyStatus> All = new()
    {
        [Draft.Value] = Draft,
        [Active.Value] = Active,
        [Suspended.Value] = Suspended,
        [Archived.Value] = Archived
    };

    public static PolicyStatus From(string value)
    {
        if (!All.TryGetValue(value, out var status))
            throw new ArgumentException($"Unknown policy status: {value}");
        return status;
    }

    public bool CanTransitionTo(PolicyStatus target)
    {
        if (this == Draft) return target == Active;
        if (this == Active) return target == Suspended || target == Archived;
        if (this == Suspended) return target == Active || target == Archived;
        return false;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
