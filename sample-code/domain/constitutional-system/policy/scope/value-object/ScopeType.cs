namespace Whycespace.Domain.ConstitutionalSystem.Policy.Scope;

using Whycespace.Domain.SharedKernel;

public sealed class ScopeType : ValueObject
{
    public string Value { get; }

    private ScopeType(string value) => Value = value;

    public static readonly ScopeType Cluster = new("cluster");
    public static readonly ScopeType Entity = new("entity");
    public static readonly ScopeType Jurisdiction = new("jurisdiction");
    public static readonly ScopeType Global = new("global");

    private static readonly Dictionary<string, ScopeType> All = new()
    {
        [Cluster.Value] = Cluster,
        [Entity.Value] = Entity,
        [Jurisdiction.Value] = Jurisdiction,
        [Global.Value] = Global
    };

    public static ScopeType From(string value)
    {
        if (!All.TryGetValue(value, out var type))
            throw new ArgumentException($"Unknown scope type: {value}");
        return type;
    }

    public bool IsNarrowerThan(ScopeType other)
    {
        return Rank(this) < Rank(other);
    }

    private static int Rank(ScopeType type) => type.Value switch
    {
        "entity" => 0,
        "cluster" => 1,
        "jurisdiction" => 2,
        "global" => 3,
        _ => -1
    };

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
