namespace Whycespace.Shared.Contracts.Control.AccessControl.Permission;

public sealed record PermissionReadModel
{
    public Guid PermissionId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ResourceScope { get; init; } = string.Empty;
    public string Actions { get; init; } = string.Empty;
    public bool IsDeprecated { get; init; }
}
