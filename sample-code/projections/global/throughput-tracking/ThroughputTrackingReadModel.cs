namespace Whycespace.Projections.Global.ThroughputTracking;

public sealed record ThroughputTrackingReadModel
{
    public required string Id { get; init; }
    public required string RegionId { get; init; }
    public long CommandsProcessed { get; init; }
    public long EventsPublished { get; init; }
    public long TransactionsCompleted { get; init; }
    public decimal CommandsPerSecond { get; init; }
    public decimal EventsPerSecond { get; init; }
    public DateTimeOffset WindowStart { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public DateTimeOffset LastEventTimestamp { get; init; }
    public long LastEventVersion { get; init; }
}
