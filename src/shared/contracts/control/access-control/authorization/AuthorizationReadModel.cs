namespace Whycespace.Shared.Contracts.Control.AccessControl.Authorization;

public sealed record AuthorizationReadModel
{
    public Guid AuthorizationId { get; init; }
    public string SubjectId { get; init; } = string.Empty;
    public IReadOnlyList<string> RoleIds { get; init; } = [];
    public DateTimeOffset ValidFrom { get; init; }
    public DateTimeOffset? ValidTo { get; init; }
    public bool IsRevoked { get; init; }
}
