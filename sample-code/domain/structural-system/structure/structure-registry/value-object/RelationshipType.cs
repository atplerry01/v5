namespace Whycespace.Domain.StructuralSystem.Structure.StructureRegistry;

public sealed record RelationshipType(string Value)
{
    public static readonly RelationshipType Owns = new("OWNS");
    public static readonly RelationshipType Governs = new("GOVERNS");
    public static readonly RelationshipType ProvidesServiceTo = new("PROVIDES_SERVICE_TO");
    public static readonly RelationshipType Funds = new("FUNDS");
    public static readonly RelationshipType Operates = new("OPERATES");

    public static RelationshipType From(string value)
    {
        return value.ToUpperInvariant() switch
        {
            "OWNS" => Owns,
            "GOVERNS" => Governs,
            "PROVIDES_SERVICE_TO" => ProvidesServiceTo,
            "FUNDS" => Funds,
            "OPERATES" => Operates,
            _ => throw new ArgumentException($"Invalid RelationshipType: {value}")
        };
    }

    public override string ToString() => Value;
}
