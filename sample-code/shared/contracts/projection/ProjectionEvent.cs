namespace Whycespace.Shared.Contracts.Projection;

/// <summary>
/// Transport-agnostic event representation consumed by domain projections.
/// Mirrors the runtime event envelope without coupling to the runtime layer.
/// Domain projections (src/projections/) use this type exclusively.
/// </summary>
public sealed record ProjectionEvent
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
    /// </summary>
    public long Version { get; init; }
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();

    // Domain routing metadata
    public string? Cluster { get; init; }
    public string? SubCluster { get; init; }
    public string? App { get; init; }
    public string? Context { get; init; }
}
