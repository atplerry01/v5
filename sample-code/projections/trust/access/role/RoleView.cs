namespace Whycespace.Projections.Trust.Access.Role;

public sealed record RoleView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
