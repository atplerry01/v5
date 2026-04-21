using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Domain.StructuralSystem.Structure.ReferenceVocabularies;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(SpvStatus status)
    {
        return status == SpvStatus.Created;
    }
}

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(SpvStatus status)
    {
        return status == SpvStatus.Active;
    }
}

public sealed class CanCloseSpecification
{
    public bool IsSatisfiedBy(SpvStatus status)
    {
        return status == SpvStatus.Suspended;
    }
}

public sealed class CanReactivateSpecification
{
    public bool IsSatisfiedBy(SpvStatus status)
    {
        return status == SpvStatus.Suspended;
    }
}

public sealed class CanRetireSpecification
{
    public bool IsSatisfiedBy(SpvStatus status)
    {
        return status == SpvStatus.Active || status == SpvStatus.Suspended;
    }
}

public sealed class CanAttachUnderParentSpecification(IStructuralParentLookup lookup)
{
    public bool IsSatisfiedBy(ClusterRef parent)
        => lookup.GetState(parent) == StructuralParentState.Active;
}

public sealed class SpvScopeSpecification(IStructuralRelationshipPolicy policy)
{
    public bool IsSatisfiedBy(SpvType type)
        => policy.SpvScopeConstraint.AllowsUnderSubcluster(type);
}
