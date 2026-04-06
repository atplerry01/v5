namespace Whycespace.Shared.Contracts.Infrastructure;

public interface IEventStore
{
    Task AppendAsync(string streamId, StoredEvent @event, CancellationToken cancellationToken = default);
    Task AppendAsync(string streamId, IEnumerable<StoredEvent> events, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StoredEvent>> ReadStreamAsync(string streamId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StoredEvent>> ReadStreamAsync(string streamId, long fromVersion, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StoredEvent>> ReadAllAsync(DateTimeOffset? after = null, CancellationToken cancellationToken = default);
    Task<long> GetStreamVersionAsync(string streamId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Transport-agnostic event representation for infrastructure contracts.
/// </summary>
public sealed record StoredEvent
{
    public Guid EventId { get; init; } = Guid.Empty;
    public required string AggregateType { get; init; }
    public required string EventType { get; init; }
    public required string CorrelationId { get; init; }
    public Guid? CommandId { get; init; }
    public string? ExecutionId { get; init; }
    public object? Payload { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
}
