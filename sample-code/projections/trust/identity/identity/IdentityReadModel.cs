namespace Whycespace.Projections.Identity;

public sealed record IdentityReadModel
{
    public required string IdentityId { get; init; }
    public required string IdentityType { get; init; }
    public required string DisplayName { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ActivatedAt { get; init; }
    public DateTimeOffset? DeactivatedAt { get; init; }
    public string? TrustLevel { get; init; }
    public int RoleCount { get; init; }
    public int PermissionCount { get; init; }
    public int DeviceCount { get; init; }
    public int ActiveSessionCount { get; init; }
}
