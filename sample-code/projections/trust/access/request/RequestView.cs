namespace Whycespace.Projections.Trust.Access.Request;

public sealed record RequestView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
