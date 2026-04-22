using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.AccessControl.Role;

public sealed record DefineRoleCommand(
    Guid RoleId,
    string Name,
    IReadOnlyList<string> PermissionIds,
    string? ParentRoleId = null) : IHasAggregateId
{
    public Guid AggregateId => RoleId;
}

public sealed record AddRolePermissionCommand(
    Guid RoleId,
    string PermissionId) : IHasAggregateId
{
    public Guid AggregateId => RoleId;
}

public sealed record DeprecateRoleCommand(
    Guid RoleId) : IHasAggregateId
{
    public Guid AggregateId => RoleId;
}
