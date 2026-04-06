namespace Whycespace.Projections.Constitutional.Policy.Violation;

public sealed record ViolationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
