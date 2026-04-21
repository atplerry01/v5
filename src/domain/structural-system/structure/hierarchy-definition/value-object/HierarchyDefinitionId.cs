using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.HierarchyDefinition;

public readonly record struct HierarchyDefinitionId
{
    public Guid Value { get; }

    public HierarchyDefinitionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "HierarchyDefinitionId cannot be empty.");
        Value = value;
    }
}
