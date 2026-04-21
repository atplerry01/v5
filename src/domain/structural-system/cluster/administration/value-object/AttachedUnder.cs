using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Administration;

public readonly record struct AttachedUnder(
    ClusterRef Parent,
    StructuralParentState ParentState,
    DateTimeOffset EffectiveAt);
