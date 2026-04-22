namespace Whycespace.Shared.Contracts.Control.AccessControl.AccessPolicy;

public sealed record AccessPolicyReadModel
{
    public Guid PolicyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Scope { get; init; } = string.Empty;
    public IReadOnlyList<string> AllowedRoleIds { get; init; } = [];
    public string Status { get; init; } = string.Empty;
}
