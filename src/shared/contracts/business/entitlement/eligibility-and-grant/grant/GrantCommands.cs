using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Entitlement.EligibilityAndGrant.Grant;

public sealed record CreateGrantCommand(
    Guid GrantId,
    Guid SubjectId,
    Guid TargetId,
    string Scope,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ExpiresAt) : IHasAggregateId
{
    public Guid AggregateId => GrantId;
}

public sealed record ActivateGrantCommand(
    Guid GrantId,
    DateTimeOffset ActivatedAt) : IHasAggregateId
{
    public Guid AggregateId => GrantId;
}

public sealed record RevokeGrantCommand(
    Guid GrantId,
    DateTimeOffset RevokedAt) : IHasAggregateId
{
    public Guid AggregateId => GrantId;
}

public sealed record ExpireGrantCommand(
    Guid GrantId,
    DateTimeOffset ExpiredAt) : IHasAggregateId
{
    public Guid AggregateId => GrantId;
}
