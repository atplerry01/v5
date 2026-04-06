namespace Whycespace.Projections.Structural.Humancapital.Stewardship;

public sealed record StewardshipView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
