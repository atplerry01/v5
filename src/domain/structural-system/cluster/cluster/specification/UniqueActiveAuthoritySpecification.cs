using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Cluster;

public sealed class UniqueActiveAuthoritySpecification
{
    public bool IsSatisfiedBy(IReadOnlyCollection<ClusterAuthorityRef> currentActive, ClusterAuthorityRef incoming)
        => !currentActive.Contains(incoming);
}
