namespace Whycespace.Domain.ConstitutionalSystem.Policy.Violation;

using Whycespace.Domain.SharedKernel;

public sealed class ViolationSeverity : ValueObject
{
    public string Value { get; }

    private ViolationSeverity(string value) => Value = value;

    public static readonly ViolationSeverity Info = new("info");
    public static readonly ViolationSeverity Warning = new("warning");
    public static readonly ViolationSeverity Critical = new("critical");
    public static readonly ViolationSeverity Fatal = new("fatal");

    private static readonly Dictionary<string, ViolationSeverity> All = new()
    {
        [Info.Value] = Info,
        [Warning.Value] = Warning,
        [Critical.Value] = Critical,
        [Fatal.Value] = Fatal
    };

    public static ViolationSeverity From(string value)
    {
        if (!All.TryGetValue(value, out var severity))
            throw new ArgumentException($"Unknown violation severity: {value}");
        return severity;
    }

    public bool IsHigherThan(ViolationSeverity other)
    {
        return Rank(this) > Rank(other);
    }

    private static int Rank(ViolationSeverity severity) => severity.Value switch
    {
        "info" => 0,
        "warning" => 1,
        "critical" => 2,
        "fatal" => 3,
        _ => -1
    };

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
