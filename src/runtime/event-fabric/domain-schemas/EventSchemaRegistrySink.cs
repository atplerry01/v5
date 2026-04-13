namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Concrete <see cref="ISchemaSink"/> over <see cref="EventSchemaRegistry"/>.
/// One-line passthrough — no transformation, no caching, no policy. The
/// indirection exists solely so per-domain schema modules can be unit-tested
/// against an interface and so the registry remains the single canonical sink.
/// </summary>
public sealed class EventSchemaRegistrySink : ISchemaSink
{
    private readonly EventSchemaRegistry _registry;

    public EventSchemaRegistrySink(EventSchemaRegistry registry)
    {
        _registry = registry;
    }

    public void RegisterSchema(string eventTypeName, EventVersion version, System.Type storedEventType, System.Type inboundEventType)
        => _registry.Register(eventTypeName, version, storedEventType, inboundEventType);

    public void RegisterPayloadMapper(string eventTypeName, System.Func<object, object> mapper)
        => _registry.RegisterPayloadMapper(eventTypeName, mapper);
}
