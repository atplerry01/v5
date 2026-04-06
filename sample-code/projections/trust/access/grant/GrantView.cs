namespace Whycespace.Projections.Trust.Access.Grant;

public sealed record GrantView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
