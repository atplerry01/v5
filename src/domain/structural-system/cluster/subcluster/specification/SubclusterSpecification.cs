namespace Whycespace.Domain.StructuralSystem.Cluster.Subcluster;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(SubclusterStatus status)
    {
        return status == SubclusterStatus.Defined;
    }
}

public sealed class CanArchiveSpecification
{
    public bool IsSatisfiedBy(SubclusterStatus status)
    {
        return status == SubclusterStatus.Active;
    }
}
