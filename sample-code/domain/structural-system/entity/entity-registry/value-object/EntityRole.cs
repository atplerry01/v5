namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed class EntityRole : IEquatable<EntityRole>
{
    public static readonly EntityRole Investor = new("INVESTOR");
    public static readonly EntityRole Operator = new("OPERATOR");
    public static readonly EntityRole Brand = new("BRAND");

    public string Value { get; }

    private EntityRole(string value)
    {
        Value = value;
    }

    public static EntityRole From(string value)
    {
        return value.ToUpperInvariant() switch
        {
            "INVESTOR" => Investor,
            "OPERATOR" => Operator,
            "BRAND" => Brand,
            _ => throw new ArgumentException($"Invalid EntityRole: {value}")
        };
    }

    public bool Equals(EntityRole? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is EntityRole other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
}
