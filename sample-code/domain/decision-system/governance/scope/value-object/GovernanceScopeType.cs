namespace Whycespace.Domain.DecisionSystem.Governance.Scope;

using Whycespace.Domain.SharedKernel;

public sealed class GovernanceScopeType : ValueObject
{
    public string Value { get; }

    private GovernanceScopeType(string value) => Value = value;

    public static readonly GovernanceScopeType Cluster = new("cluster");
    public static readonly GovernanceScopeType SPV = new("spv");
    public static readonly GovernanceScopeType Global = new("global");

    private static readonly Dictionary<string, GovernanceScopeType> All = new()
    {
        [Cluster.Value] = Cluster,
        [SPV.Value] = SPV,
        [Global.Value] = Global
    };

    public static GovernanceScopeType From(string value)
    {
        if (!All.TryGetValue(value, out var type))
            throw new ArgumentException($"Unknown governance scope type: {value}");
        return type;
    }

    public bool IsNarrowerThan(GovernanceScopeType other)
    {
        return Rank(this) < Rank(other);
    }

    private static int Rank(GovernanceScopeType type) => type.Value switch
    {
        "cluster" => 0,
        "spv" => 1,
        "global" => 2,
        _ => -1
    };

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
