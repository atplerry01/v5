using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Domain.StructuralSystem.Structure.ReferenceVocabularies;

namespace Whycespace.Domain.StructuralSystem.Cluster.Provider;

public sealed class CanActivateSpecification
{
    public bool IsSatisfiedBy(ProviderStatus status)
    {
        return status == ProviderStatus.Registered;
    }
}

public sealed class CanSuspendSpecification
{
    public bool IsSatisfiedBy(ProviderStatus status)
    {
        return status == ProviderStatus.Active;
    }
}

public sealed class CanReactivateSpecification
{
    public bool IsSatisfiedBy(ProviderStatus status)
    {
        return status == ProviderStatus.Suspended;
    }
}

public sealed class CanRetireSpecification
{
    public bool IsSatisfiedBy(ProviderStatus status)
    {
        return status == ProviderStatus.Active || status == ProviderStatus.Suspended;
    }
}

public sealed class CanAttachUnderParentSpecification(IStructuralParentLookup lookup)
{
    public bool IsSatisfiedBy(ClusterRef parent)
        => lookup.GetState(parent) == StructuralParentState.Active;
}

public sealed class CanBindProviderToAuthoritySpecification(IStructuralRelationshipPolicy policy)
{
    public bool IsSatisfiedBy(AuthorityRole authorityRole, ProviderCategory providerCategory)
        => policy.AuthorityProviderMatrix.IsAllowed(authorityRole, providerCategory);
}
