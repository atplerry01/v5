namespace Whycespace.Projections.Trust.Access.Permission;

public sealed record PermissionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
