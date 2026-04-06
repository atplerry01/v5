namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

using Whycespace.Domain.SharedKernel;

public sealed class FederationRelationType : ValueObject
{
    public string Value { get; }

    private FederationRelationType(string value) => Value = value;

    public static readonly FederationRelationType DependsOn = new("depends_on");
    public static readonly FederationRelationType Overrides = new("overrides");
    public static readonly FederationRelationType Constrains = new("constrains");

    private static readonly Dictionary<string, FederationRelationType> All = new()
    {
        [DependsOn.Value] = DependsOn,
        [Overrides.Value] = Overrides,
        [Constrains.Value] = Constrains
    };

    public static FederationRelationType From(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (!All.TryGetValue(value, out var type))
            throw new ArgumentException($"Invalid federation relation type: {value}");
        return type;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
