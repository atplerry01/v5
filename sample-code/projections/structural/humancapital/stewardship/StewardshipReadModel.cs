namespace Whycespace.Projections.Structural.Humancapital.Stewardship;

public sealed record StewardshipReadModel
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public DateTimeOffset LastEventTimestamp { get; init; }
    public long LastEventVersion { get; init; }
}
