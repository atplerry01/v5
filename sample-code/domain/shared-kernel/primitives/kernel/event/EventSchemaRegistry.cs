namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

/// <summary>
/// Domain-level registry of event schemas. Tracks all known event types and their versions.
/// Used during replay to resolve the correct deserializer and upgrade path.
/// </summary>
public sealed class EventSchemaRegistry
{
    private readonly Dictionary<string, SortedList<int, EventSchemaRegistration>> _schemas = new();

    /// <summary>All schema IDs known to this registry.</summary>
    public IReadOnlyCollection<string> RegisteredTypes => _schemas.Keys;

    /// <summary>
    /// Registers a schema version. Throws if the same schemaId+version is already registered
    /// with a different type, enforcing immutability of persisted schemas.
    /// </summary>
    public void Register(EventSchemaRegistration registration)
    {
        if (!_schemas.TryGetValue(registration.SchemaId, out var versions))
        {
            versions = new SortedList<int, EventSchemaRegistration>();
            _schemas[registration.SchemaId] = versions;
        }

        if (versions.TryGetValue(registration.Version, out var existing))
        {
            if (existing.EventClrType != registration.EventClrType)
                throw new DomainException(
                    "EVENT_SCHEMA_CONFLICT",
                    $"Schema '{registration.SchemaId}' v{registration.Version} is already registered to '{existing.EventClrType.Name}'. " +
                    "Persisted event schemas are immutable — register a new version instead.");
            return; // idempotent re-registration
        }

        versions.Add(registration.Version, registration);
    }

    /// <summary>Resolves the registration for a specific schema version.</summary>
    public EventSchemaRegistration? Resolve(string schemaId, int version)
    {
        if (_schemas.TryGetValue(schemaId, out var versions) &&
            versions.TryGetValue(version, out var registration))
            return registration;

        return null;
    }

    /// <summary>Returns the latest registered version for a schema, or null if unknown.</summary>
    public EventSchemaRegistration? ResolveLatest(string schemaId)
    {
        if (_schemas.TryGetValue(schemaId, out var versions) && versions.Count > 0)
            return versions.Values[^1];

        return null;
    }

    /// <summary>Returns all registered versions for a schema, ordered ascending.</summary>
    public IReadOnlyList<EventSchemaRegistration> GetVersionHistory(string schemaId)
    {
        if (_schemas.TryGetValue(schemaId, out var versions))
            return versions.Values.ToList().AsReadOnly();

        return Array.Empty<EventSchemaRegistration>();
    }
}

/// <summary>
/// Represents a single registered event schema version.
/// </summary>
public sealed record EventSchemaRegistration
{
    public required string SchemaId { get; init; }
    public required int Version { get; init; }
    public required Type EventClrType { get; init; }
    public required EventSchemaVersion Schema { get; init; }
}
