using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Cluster.Subcluster;

public sealed record DefineSubclusterCommand(
    Guid SubclusterId,
    Guid ParentClusterReference,
    string SubclusterName) : IHasAggregateId
{
    public Guid AggregateId => SubclusterId;
}

public sealed record DefineSubclusterWithParentCommand(
    Guid SubclusterId,
    Guid ParentClusterReference,
    string SubclusterName,
    DateTimeOffset EffectiveAt) : IHasAggregateId
{
    public Guid AggregateId => SubclusterId;
}

public sealed record ActivateSubclusterCommand(
    Guid SubclusterId) : IHasAggregateId
{
    public Guid AggregateId => SubclusterId;
}

public sealed record SuspendSubclusterCommand(
    Guid SubclusterId) : IHasAggregateId
{
    public Guid AggregateId => SubclusterId;
}

public sealed record ReactivateSubclusterCommand(
    Guid SubclusterId) : IHasAggregateId
{
    public Guid AggregateId => SubclusterId;
}

public sealed record ArchiveSubclusterCommand(
    Guid SubclusterId) : IHasAggregateId
{
    public Guid AggregateId => SubclusterId;
}

public sealed record RetireSubclusterCommand(
    Guid SubclusterId) : IHasAggregateId
{
    public Guid AggregateId => SubclusterId;
}
