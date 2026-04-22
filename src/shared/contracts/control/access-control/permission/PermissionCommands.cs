using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.AccessControl.Permission;

public sealed record DefinePermissionCommand(
    Guid PermissionId,
    string Name,
    string ResourceScope,
    string Actions) : IHasAggregateId
{
    public Guid AggregateId => PermissionId;
}

public sealed record DeprecatePermissionCommand(
    Guid PermissionId) : IHasAggregateId
{
    public Guid AggregateId => PermissionId;
}
