namespace Whycespace.Domain.StructuralSystem.Structure.TypeDefinition;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(TypeDefinitionStatus status)
    {
        return status == TypeDefinitionStatus.Defined;
    }
}

public sealed class CanRetireSpecification
{
    public bool IsSatisfiedBy(TypeDefinitionStatus status)
    {
        return status == TypeDefinitionStatus.Active;
    }
}
