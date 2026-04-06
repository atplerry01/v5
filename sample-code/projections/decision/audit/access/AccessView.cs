namespace Whycespace.Projections.Decision.Audit.Access;

public sealed record AccessView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
