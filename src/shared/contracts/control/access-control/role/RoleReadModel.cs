namespace Whycespace.Shared.Contracts.Control.AccessControl.Role;

public sealed record RoleReadModel
{
    public Guid RoleId { get; init; }
    public string Name { get; init; } = string.Empty;
    public IReadOnlyList<string> PermissionIds { get; init; } = [];
    public string? ParentRoleId { get; init; }
    public bool IsDeprecated { get; init; }
}
