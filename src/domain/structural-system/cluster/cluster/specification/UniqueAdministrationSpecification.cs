using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Cluster;

public sealed class UniqueAdministrationSpecification
{
    public bool IsSatisfiedBy(IReadOnlyCollection<ClusterAdministrationRef> currentActive, ClusterAdministrationRef incoming)
        => !currentActive.Contains(incoming);
}
