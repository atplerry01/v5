using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.HierarchyDefinition;

public readonly record struct HierarchyDefinitionDescriptor
{
    public string HierarchyName { get; }
    public Guid ParentReference { get; }

    public HierarchyDefinitionDescriptor(string hierarchyName, Guid parentReference)
    {
        Guard.Against(string.IsNullOrWhiteSpace(hierarchyName), "HierarchyDefinitionDescriptor name must not be empty.");

        HierarchyName = hierarchyName;
        ParentReference = parentReference;
    }
}
