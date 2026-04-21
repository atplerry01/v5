using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.TypeDefinition;

public readonly record struct TypeDefinitionId
{
    public Guid Value { get; }

    public TypeDefinitionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "TypeDefinitionId cannot be empty.");
        Value = value;
    }
}
