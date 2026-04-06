namespace Whycespace.Engines.T2E.Trust.Access.Permission;

public record PermissionCommand(string Action, string EntityId, object Payload);
public sealed record CreatePermissionCommand(string PermissionId, string Name, string PermissionAction, string Resource, string Scope) : PermissionCommand(PermissionAction, PermissionId, null!);
public sealed record GrantPermissionCommand(string PermissionId, string IdentityId) : PermissionCommand("Grant", PermissionId, null!);
public sealed record RevokePermissionCommand(string PermissionId, string IdentityId) : PermissionCommand("Revoke", PermissionId, null!);
public sealed record DeactivatePermissionCommand(string PermissionId) : PermissionCommand("Deactivate", PermissionId, null!);
