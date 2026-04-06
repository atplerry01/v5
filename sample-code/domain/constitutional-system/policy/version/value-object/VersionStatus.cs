namespace Whycespace.Domain.ConstitutionalSystem.Policy.Version;

using Whycespace.Domain.SharedKernel;

public sealed class VersionStatus : ValueObject
{
    public string Value { get; }

    private VersionStatus(string value) => Value = value;

    public static readonly VersionStatus Draft = new("draft");
    public static readonly VersionStatus Active = new("active");
    public static readonly VersionStatus Superseded = new("superseded");
    public static readonly VersionStatus Archived = new("archived");

    private static readonly Dictionary<string, VersionStatus> All = new()
    {
        [Draft.Value] = Draft,
        [Active.Value] = Active,
        [Superseded.Value] = Superseded,
        [Archived.Value] = Archived
    };

    public static VersionStatus From(string value)
    {
        if (!All.TryGetValue(value, out var status))
            throw new ArgumentException($"Unknown version status: {value}");
        return status;
    }

    public bool CanTransitionTo(VersionStatus target)
    {
        if (this == Draft) return target == Active;
        if (this == Active) return target == Superseded || target == Archived;
        if (this == Superseded) return target == Archived;
        return false;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
