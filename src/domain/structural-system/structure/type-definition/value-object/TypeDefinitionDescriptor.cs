namespace Whycespace.Domain.StructuralSystem.Structure.TypeDefinition;

public readonly record struct TypeDefinitionDescriptor
{
    public string TypeName { get; }
    public string TypeCategory { get; }

    public TypeDefinitionDescriptor(string typeName, string typeCategory)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            throw TypeDefinitionErrors.MissingDescriptor();

        if (string.IsNullOrWhiteSpace(typeCategory))
            throw TypeDefinitionErrors.MissingDescriptor();

        TypeName = typeName;
        TypeCategory = typeCategory;
    }
}
