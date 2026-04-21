using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Administration;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(AdministrationStatus status)
    {
        return status == AdministrationStatus.Established;
    }
}

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(AdministrationStatus status)
    {
        return status == AdministrationStatus.Active;
    }
}

public sealed class CanRetireSpecification
{
    public bool IsSatisfiedBy(AdministrationStatus status)
    {
        return status == AdministrationStatus.Active || status == AdministrationStatus.Suspended;
    }
}

public sealed class CanAttachUnderParentSpecification(IStructuralParentLookup lookup)
{
    public bool IsSatisfiedBy(ClusterRef parent)
        => lookup.GetState(parent) == StructuralParentState.Active;
}
