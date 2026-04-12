namespace Whycespace.Domain.StructuralSystem.Structure.TypeDefinition;

public readonly record struct TypeDefinitionId
{
    public Guid Value { get; }

    public TypeDefinitionId(Guid value)
    {
        if (value == Guid.Empty)
            throw TypeDefinitionErrors.MissingId();

        Value = value;
    }
}
