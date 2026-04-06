using System.Collections.Concurrent;
using Whyce.Runtime.Deterministic;

namespace Whyce.Runtime.EventFabric;

/// <summary>
/// Event Schema Registry — maintains versioned schema metadata for all event types.
/// Every event type MUST be registered before it can be processed by the Event Fabric.
///
/// Provides:
/// - Event name resolution
/// - Event version tracking
/// - Schema hash computation (deterministic)
/// </summary>
public sealed class EventSchemaRegistry
{
    private readonly ConcurrentDictionary<string, EventSchemaEntry> _schemas = new();
    private readonly ConcurrentDictionary<string, Func<object, object>> _payloadMappers = new();
    private bool _locked;

    /// <summary>
    /// Registers an event type with its version.
    /// </summary>
    public void Register(string eventTypeName, EventVersion version)
    {
        if (_locked)
            throw new InvalidOperationException("EventSchemaRegistry is locked. Cannot register after build.");

        var schemaHash = DeterministicHasher.ComputeHash($"{eventTypeName}:{version}");
        _schemas[eventTypeName] = new EventSchemaEntry
        {
            EventName = eventTypeName,
            Version = version,
            SchemaHash = schemaHash
        };
    }

    /// <summary>
    /// Registers an event type at default version (1.0.0).
    /// </summary>
    public void Register(string eventTypeName)
    {
        Register(eventTypeName, EventVersion.Default);
    }

    /// <summary>
    /// Registers a payload mapper that converts a domain event to its shared schema contract.
    /// Used by EventFabric to map Payload before dispatching to projections.
    /// </summary>
    public void RegisterPayloadMapper(string eventTypeName, Func<object, object> mapper)
    {
        if (_locked)
            throw new InvalidOperationException("EventSchemaRegistry is locked. Cannot register after build.");

        _payloadMappers[eventTypeName] = mapper;
    }

    /// <summary>
    /// Maps a domain event to its shared schema contract.
    /// If no mapper is registered, returns the original event unchanged.
    /// </summary>
    public object MapPayload(string eventTypeName, object domainEvent)
    {
        return _payloadMappers.TryGetValue(eventTypeName, out var mapper)
            ? mapper(domainEvent)
            : domainEvent;
    }

    /// <summary>
    /// Resolves schema entry for an event type. Returns default if not registered.
    /// </summary>
    public EventSchemaEntry Resolve(string eventTypeName)
    {
        if (_schemas.TryGetValue(eventTypeName, out var entry))
            return entry;

        return new EventSchemaEntry
        {
            EventName = eventTypeName,
            Version = EventVersion.Default,
            SchemaHash = DeterministicHasher.ComputeHash($"{eventTypeName}:{EventVersion.Default}")
        };
    }

    public void Lock() => _locked = true;
}

public sealed record EventSchemaEntry
{
    public required string EventName { get; init; }
    public required EventVersion Version { get; init; }
    public required string SchemaHash { get; init; }
}
