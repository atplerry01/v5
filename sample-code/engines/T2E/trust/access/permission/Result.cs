namespace Whycespace.Engines.T2E.Trust.Access.Permission;

public record PermissionResult(bool Success, string Message);
public sealed record PermissionDto(string PermissionId, string Name, string Action, string Resource, string Status);
