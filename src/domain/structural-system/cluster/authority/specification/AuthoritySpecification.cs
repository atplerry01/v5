namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(AuthorityStatus status)
    {
        return status == AuthorityStatus.Established;
    }
}

public sealed class CanRevokeSpecification
{
    public bool IsSatisfiedBy(AuthorityStatus status)
    {
        return status == AuthorityStatus.Active;
    }
}
