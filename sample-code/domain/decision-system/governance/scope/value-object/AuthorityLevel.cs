namespace Whycespace.Domain.DecisionSystem.Governance.Scope;

using Whycespace.Domain.SharedKernel;

public sealed class AuthorityLevel : ValueObject
{
    public string Value { get; }

    private AuthorityLevel(string value) => Value = value;

    public static readonly AuthorityLevel Full = new("full");
    public static readonly AuthorityLevel Limited = new("limited");
    public static readonly AuthorityLevel Advisory = new("advisory");

    private static readonly Dictionary<string, AuthorityLevel> All = new()
    {
        [Full.Value] = Full,
        [Limited.Value] = Limited,
        [Advisory.Value] = Advisory
    };

    public static AuthorityLevel From(string value)
    {
        if (!All.TryGetValue(value, out var type))
            throw new ArgumentException($"Unknown authority level: {value}");
        return type;
    }

    public bool HasMoreAuthorityThan(AuthorityLevel other)
    {
        return Rank(this) > Rank(other);
    }

    private static int Rank(AuthorityLevel level) => level.Value switch
    {
        "advisory" => 0,
        "limited" => 1,
        "full" => 2,
        _ => -1
    };

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
