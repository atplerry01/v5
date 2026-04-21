using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.TypeDefinition;

public readonly record struct TypeDefinitionDescriptor
{
    public string TypeName { get; }
    public string TypeCategory { get; }

    public TypeDefinitionDescriptor(string typeName, string typeCategory)
    {
        Guard.Against(string.IsNullOrWhiteSpace(typeName), "TypeDefinitionDescriptor name must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(typeCategory), "TypeDefinitionDescriptor category must not be empty.");

        TypeName = typeName;
        TypeCategory = typeCategory;
    }
}
