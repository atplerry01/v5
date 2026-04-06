namespace Whycespace.Projections.Core.Temporal.Timeline;

public sealed record TimelineView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
