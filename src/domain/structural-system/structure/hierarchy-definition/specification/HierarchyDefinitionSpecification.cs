namespace Whycespace.Domain.StructuralSystem.Structure.HierarchyDefinition;

public sealed class CanValidateSpecification
{
    public bool IsSatisfiedBy(HierarchyDefinitionStatus status)
    {
        return status == HierarchyDefinitionStatus.Defined;
    }
}

public sealed class CanLockSpecification
{
    public bool IsSatisfiedBy(HierarchyDefinitionStatus status)
    {
        return status == HierarchyDefinitionStatus.Validated;
    }
}
