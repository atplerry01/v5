namespace Whycespace.Shared.Contracts.Event;

/// <summary>
/// Contract for event schema registration and resolution.
/// Infrastructure and shared layers depend on this interface;
/// domain provides the implementation.
/// </summary>
public interface IEventSchemaRegistry
{
    IReadOnlyCollection<string> RegisteredTypes { get; }
    void Register(EventSchemaRegistrationRecord registration);
    EventSchemaRegistrationRecord? Resolve(string schemaId, int version);
    EventSchemaRegistrationRecord? ResolveLatest(string schemaId);
    IReadOnlyList<EventSchemaRegistrationRecord> GetVersionHistory(string schemaId);
}

/// <summary>
/// Represents a single registered event schema version.
/// </summary>
public sealed record EventSchemaRegistrationRecord
{
    public required string SchemaId { get; init; }
    public required int Version { get; init; }
    public required Type EventClrType { get; init; }
    public required EventSchemaVersionRecord Schema { get; init; }
}

/// <summary>
/// Describes a specific version of an event schema.
/// </summary>
public sealed record EventSchemaVersionRecord
{
    public required string SchemaId { get; init; }
    public required int Version { get; init; }
    public required IReadOnlyList<EventFieldDescriptorRecord> Fields { get; init; }
}

/// <summary>
/// Describes a single field within an event schema version.
/// </summary>
public sealed record EventFieldDescriptorRecord
{
    public required string Name { get; init; }
    public required string TypeName { get; init; }
    public bool IsRequired { get; init; } = true;
}
