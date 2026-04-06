namespace Whycespace.Projections.Core.Event.EventStream;

public sealed record EventStreamView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
