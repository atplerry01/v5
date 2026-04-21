using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Structural.Cluster.Authority;

public sealed record EstablishAuthorityCommand(
    Guid AuthorityId,
    Guid ClusterReference,
    string AuthorityName) : IHasAggregateId
{
    public Guid AggregateId => AuthorityId;
}

public sealed record EstablishAuthorityWithParentCommand(
    Guid AuthorityId,
    Guid ClusterReference,
    string AuthorityName,
    DateTimeOffset EffectiveAt) : IHasAggregateId
{
    public Guid AggregateId => AuthorityId;
}

public sealed record ActivateAuthorityCommand(
    Guid AuthorityId) : IHasAggregateId
{
    public Guid AggregateId => AuthorityId;
}

public sealed record RevokeAuthorityCommand(
    Guid AuthorityId) : IHasAggregateId
{
    public Guid AggregateId => AuthorityId;
}

public sealed record SuspendAuthorityCommand(
    Guid AuthorityId) : IHasAggregateId
{
    public Guid AggregateId => AuthorityId;
}

public sealed record ReactivateAuthorityCommand(
    Guid AuthorityId) : IHasAggregateId
{
    public Guid AggregateId => AuthorityId;
}

public sealed record RetireAuthorityCommand(
    Guid AuthorityId) : IHasAggregateId
{
    public Guid AggregateId => AuthorityId;
}
