namespace Whycespace.Domain.StructuralSystem.Structure.HierarchyDefinition;

public readonly record struct HierarchyDefinitionId
{
    public Guid Value { get; }

    public HierarchyDefinitionId(Guid value)
    {
        if (value == Guid.Empty)
            throw HierarchyDefinitionErrors.MissingId();

        Value = value;
    }
}
