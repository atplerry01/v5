namespace Whycespace.Domain.StructuralSystem.Structure.StructureRegistry;

public sealed record StructureName
{
    public string Value { get; }

    public StructureName(string value)
    {
        Guard.AgainstEmpty(value);
        Value = value;
    }

    public override string ToString() => Value;
}
