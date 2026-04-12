namespace Whycespace.Domain.StructuralSystem.Structure.HierarchyDefinition;

public readonly record struct HierarchyDefinitionDescriptor
{
    public string HierarchyName { get; }
    public Guid ParentReference { get; }

    public HierarchyDefinitionDescriptor(string hierarchyName, Guid parentReference)
    {
        if (string.IsNullOrWhiteSpace(hierarchyName))
            throw HierarchyDefinitionErrors.MissingDescriptor();

        HierarchyName = hierarchyName;
        ParentReference = parentReference;
    }
}
