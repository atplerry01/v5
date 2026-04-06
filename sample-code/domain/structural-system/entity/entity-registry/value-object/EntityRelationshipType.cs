namespace Whycespace.Domain.StructuralSystem.Entity.EntityRegistry;

public sealed class EntityRelationshipType : IEquatable<EntityRelationshipType>
{
    public static readonly EntityRelationshipType Owns = new("OWNS");
    public static readonly EntityRelationshipType Governs = new("GOVERNS");
    public static readonly EntityRelationshipType ProvidesServiceTo = new("PROVIDES_SERVICE_TO");
    public static readonly EntityRelationshipType Funds = new("FUNDS");
    public static readonly EntityRelationshipType Operates = new("OPERATES");
    public static readonly EntityRelationshipType DeliversTo = new("DELIVERS_TO");

    public string Value { get; }

    private EntityRelationshipType(string value)
    {
        Value = value;
    }

    public static EntityRelationshipType From(string value)
    {
        return value.ToUpperInvariant() switch
        {
            "OWNS" => Owns,
            "GOVERNS" => Governs,
            "PROVIDES_SERVICE_TO" => ProvidesServiceTo,
            "FUNDS" => Funds,
            "OPERATES" => Operates,
            "DELIVERS_TO" => DeliversTo,
            _ => throw new ArgumentException($"Invalid relationship type: {value}")
        };
    }

    public bool Equals(EntityRelationshipType? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is EntityRelationshipType other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
}
