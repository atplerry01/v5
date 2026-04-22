namespace Whycespace.Shared.Contracts.Control.AccessControl.Principal;

public sealed record PrincipalReadModel
{
    public Guid PrincipalId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string IdentityId { get; init; } = string.Empty;
    public IReadOnlyList<string> RoleIds { get; init; } = [];
    public string Status { get; init; } = string.Empty;
}
