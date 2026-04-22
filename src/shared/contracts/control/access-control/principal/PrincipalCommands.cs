using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.AccessControl.Principal;

public sealed record RegisterPrincipalCommand(
    Guid PrincipalId,
    string Name,
    string Kind,
    string IdentityId) : IHasAggregateId
{
    public Guid AggregateId => PrincipalId;
}

public sealed record AssignPrincipalRoleCommand(
    Guid PrincipalId,
    string RoleId) : IHasAggregateId
{
    public Guid AggregateId => PrincipalId;
}

public sealed record DeactivatePrincipalCommand(
    Guid PrincipalId) : IHasAggregateId
{
    public Guid AggregateId => PrincipalId;
}
