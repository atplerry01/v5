namespace Whycespace.Domain.StructuralSystem.Structure.StructureRegistry;

public sealed record StructureType(string Value)
{
    public static readonly StructureType Global = new("GLOBAL");
    public static readonly StructureType Holding = new("HOLDING");
    public static readonly StructureType Cluster = new("CLUSTER");
    public static readonly StructureType Authority = new("AUTHORITY");
    public static readonly StructureType Provider = new("PROVIDER");
    public static readonly StructureType Spv = new("SPV");

    public static StructureType From(string value)
    {
        return value.ToUpperInvariant() switch
        {
            "GLOBAL" => Global,
            "HOLDING" => Holding,
            "CLUSTER" => Cluster,
            "AUTHORITY" => Authority,
            "PROVIDER" => Provider,
            "SPV" => Spv,
            _ => throw new ArgumentException($"Invalid StructureType: {value}")
        };
    }

    public override string ToString() => Value;
}
