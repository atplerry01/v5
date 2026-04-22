namespace Whycespace.Shared.Contracts.Events.Control.AccessControl.Role;

public sealed record RoleDefinedEventSchema(
    Guid AggregateId,
    string Name,
    IReadOnlyList<string> PermissionIds,
    string? ParentRoleId);

public sealed record RolePermissionAddedEventSchema(
    Guid AggregateId,
    string PermissionId);

public sealed record RoleDeprecatedEventSchema(
    Guid AggregateId);
