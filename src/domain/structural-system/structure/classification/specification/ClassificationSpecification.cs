namespace Whycespace.Domain.StructuralSystem.Structure.Classification;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ClassificationStatus status)
    {
        return status == ClassificationStatus.Defined;
    }
}

public sealed class CanDeprecateSpecification
{
    public bool IsSatisfiedBy(ClassificationStatus status)
    {
        return status == ClassificationStatus.Active;
    }
}
