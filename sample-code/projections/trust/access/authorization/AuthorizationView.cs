namespace Whycespace.Projections.Trust.Access.Authorization;

public sealed record AuthorizationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
