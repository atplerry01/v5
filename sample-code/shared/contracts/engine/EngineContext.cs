namespace Whycespace.Shared.Contracts.Engine;

/// <summary>
/// Execution context provided to engines during command processing.
/// Exposes only LoadAggregate and EmitEvents — no repository, SaveChanges, DbContext, or SQL.
///
/// The aggregate store is internal and pluggable — the runtime provides the
/// actual implementation via IAggregateStore injection.
/// </summary>
public class EngineContext
{
    public required string CorrelationId { get; init; }
    public required string CommandType { get; init; }
    public required object Payload { get; init; }
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();

    private readonly IAggregateStore _aggregateStore;
    private readonly IValidationGate _validationGate;
    private readonly List<object> _emittedEvents = [];

    /// <summary>
    /// Domain events collected from aggregates during this execution cycle.
    /// </summary>
    public IReadOnlyList<object> EmittedEvents => _emittedEvents.AsReadOnly();

    public EngineContext()
    {
        _aggregateStore = new InMemoryAggregateStore();
        _validationGate = PassThroughValidationGate.Instance;
    }

    public EngineContext(IAggregateStore aggregateStore)
    {
        _aggregateStore = aggregateStore;
        _validationGate = PassThroughValidationGate.Instance;
    }

    public EngineContext(IAggregateStore aggregateStore, IValidationGate validationGate)
    {
        _aggregateStore = aggregateStore;
        _validationGate = validationGate;
    }

    public Task<T> LoadAggregate<T>(string id) where T : class, new()
    {
        return _aggregateStore.LoadAsync<T>(id);
    }

    /// <summary>
    /// Persists the aggregate and collects its pending domain events.
    /// This is the ONLY write path — engines must not access the store directly.
    /// </summary>
    public async Task EmitEvents<T>(T aggregate) where T : class, IEventSource
    {
        _emittedEvents.AddRange(aggregate.GetPendingEvents());
        aggregate.ClearPendingEvents();
        await _aggregateStore.SaveAsync(aggregate);
    }

    /// <summary>
    /// Calls the T0U validation gate before T2E proceeds with execution.
    /// Returns null if valid, or the validation failure reason.
    /// </summary>
    public async Task<ValidationResult> ValidateAsync(string entityId, CancellationToken cancellationToken = default)
    {
        return await _validationGate.ValidateAsync(CommandType, entityId, cancellationToken);
    }
}

public interface IAggregateStore
{
    Task<T> LoadAsync<T>(string id) where T : class, new();
    Task SaveAsync<T>(T aggregate) where T : class;
}

public sealed class InMemoryAggregateStore : IAggregateStore
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, object> _store = new();

    public Task<T> LoadAsync<T>(string id) where T : class, new()
    {
        if (_store.TryGetValue(id, out var existing) && existing is T typed)
            return Task.FromResult(typed);

        var instance = new T();
        _store[id] = instance;
        return Task.FromResult(instance);
    }

    public Task SaveAsync<T>(T aggregate) where T : class
    {
        // Extract Id via reflection for AggregateRoot-derived types
        var idProp = aggregate.GetType().GetProperty("Id");
        if (idProp?.GetValue(aggregate) is Guid id && id != Guid.Empty)
        {
            _store[id.ToString()] = aggregate;
        }
        return Task.CompletedTask;
    }
}
