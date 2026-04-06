namespace Whycespace.Engines.T2E.Trust.Access.Role;

public record RoleCommand(string Action, string EntityId, object Payload);
public sealed record CreateRoleCommand(string RoleId, string Name, string Description, string Scope) : RoleCommand("Create", RoleId, null!);
public sealed record AssignRoleCommand(string RoleId, string IdentityId) : RoleCommand("Assign", RoleId, null!);
public sealed record RevokeRoleCommand(string RoleId, string IdentityId) : RoleCommand("Revoke", RoleId, null!);
public sealed record DeactivateRoleCommand(string RoleId) : RoleCommand("Deactivate", RoleId, null!);
