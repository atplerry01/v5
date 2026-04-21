using Whycespace.Domain.StructuralSystem.Contracts.References;

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

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(AuthorityStatus status)
    {
        return status == AuthorityStatus.Active;
    }
}

public sealed class CanReactivateSpecification
{
    public bool IsSatisfiedBy(AuthorityStatus status)
    {
        return status == AuthorityStatus.Suspended;
    }
}

public sealed class CanRetireSpecification
{
    public bool IsSatisfiedBy(AuthorityStatus status)
    {
        return status == AuthorityStatus.Active || status == AuthorityStatus.Suspended;
    }
}

public sealed class CanAttachUnderParentSpecification(IStructuralParentLookup lookup)
{
    public bool IsSatisfiedBy(ClusterRef parent)
        => lookup.GetState(parent) == StructuralParentState.Active;
}
