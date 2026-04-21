using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Domain.StructuralSystem.Structure.ReferenceVocabularies;

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

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(SubclusterStatus status)
    {
        return status == SubclusterStatus.Active;
    }
}

public sealed class CanReactivateSpecification
{
    public bool IsSatisfiedBy(SubclusterStatus status)
    {
        return status == SubclusterStatus.Suspended;
    }
}

public sealed class CanRetireSpecification
{
    public bool IsSatisfiedBy(SubclusterStatus status)
    {
        return status == SubclusterStatus.Active || status == SubclusterStatus.Suspended;
    }
}

public sealed class CanAttachUnderParentSpecification(IStructuralParentLookup lookup)
{
    public bool IsSatisfiedBy(ClusterRef parent)
        => lookup.GetState(parent) == StructuralParentState.Active;
}

public sealed class CanReportSubclusterToAuthoritySpecification(IStructuralRelationshipPolicy policy)
{
    public bool IsSatisfiedBy(AuthorityRole authorityRole)
        => policy.AuthoritySubclusterConstraint.IsAllowed(authorityRole);
}
