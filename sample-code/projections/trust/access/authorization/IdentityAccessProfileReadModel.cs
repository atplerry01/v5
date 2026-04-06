namespace Whycespace.Projections.Identity;

public sealed record IdentityAccessProfileReadModel
{
    public required string ProfileId { get; init; }
    public required string IdentityId { get; init; }
    public required string AccessLevel { get; init; }
    public required string Status { get; init; }
    public int RoleCount { get; init; }
    public int PermissionCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
