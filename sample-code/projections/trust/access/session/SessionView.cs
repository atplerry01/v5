namespace Whycespace.Projections.Trust.Access.Session;

public sealed record SessionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
