namespace Whycespace.Projections.Identity;

public sealed record IdentityRoleReadModel
{
    public required string RoleId { get; init; }
    public required string Name { get; init; }
    public required string Scope { get; init; }
    public required string Status { get; init; }
    public int AssignmentCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
