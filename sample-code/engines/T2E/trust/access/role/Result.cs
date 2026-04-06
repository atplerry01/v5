namespace Whycespace.Engines.T2E.Trust.Access.Role;

public record RoleResult(bool Success, string Message);
public sealed record RoleDto(string RoleId, string Name, string Scope, string Status);
