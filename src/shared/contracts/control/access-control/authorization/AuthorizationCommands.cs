using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.AccessControl.Authorization;

public sealed record GrantAuthorizationCommand(
    Guid AuthorizationId,
    string SubjectId,
    IReadOnlyList<string> RoleIds,
    DateTimeOffset ValidFrom,
    DateTimeOffset? ValidTo = null) : IHasAggregateId
{
    public Guid AggregateId => AuthorizationId;
}

public sealed record RevokeAuthorizationCommand(
    Guid AuthorizationId) : IHasAggregateId
{
    public Guid AggregateId => AuthorizationId;
}
