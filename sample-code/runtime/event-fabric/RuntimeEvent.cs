namespace Whycespace.Runtime.EventFabric;

public sealed record RuntimeEvent
{
    public required Guid EventId { get; init; }
    public required Guid AggregateId { get; init; }
    public required string AggregateType { get; init; }
    public required string EventType { get; init; }
    public required string CorrelationId { get; init; }
    public Guid? CommandId { get; init; }
    public string? ExecutionId { get; init; }
    public object? Payload { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    /// <summary>
    /// Monotonic version per aggregate. Used for global ordering tiebreaker.
    /// If absent, derived from aggregate version at publish time.
    /// </summary>
    public long Version { get; init; }
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();

    // Domain routing metadata
    public string? Cluster { get; init; }
    public string? SubCluster { get; init; }
    public string? App { get; init; }
    public string? Context { get; init; }
}
