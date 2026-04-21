using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Cluster.Cluster;

public sealed record DefineClusterCommand(
    Guid ClusterId,
    string ClusterName,
    string ClusterType) : IHasAggregateId
{
    public Guid AggregateId => ClusterId;
}

public sealed record ActivateClusterCommand(
    Guid ClusterId) : IHasAggregateId
{
    public Guid AggregateId => ClusterId;
}

public sealed record ArchiveClusterCommand(
    Guid ClusterId) : IHasAggregateId
{
    public Guid AggregateId => ClusterId;
}

public sealed record BindAuthorityToClusterCommand(
    Guid ClusterId,
    Guid AuthorityId) : IHasAggregateId
{
    public Guid AggregateId => ClusterId;
}

public sealed record ReleaseAuthorityFromClusterCommand(
    Guid ClusterId,
    Guid AuthorityId) : IHasAggregateId
{
    public Guid AggregateId => ClusterId;
}

public sealed record BindAdministrationToClusterCommand(
    Guid ClusterId,
    Guid AdministrationId) : IHasAggregateId
{
    public Guid AggregateId => ClusterId;
}

public sealed record ReleaseAdministrationFromClusterCommand(
    Guid ClusterId,
    Guid AdministrationId) : IHasAggregateId
{
    public Guid AggregateId => ClusterId;
}
