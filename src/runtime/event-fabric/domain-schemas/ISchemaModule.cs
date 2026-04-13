namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Per-domain schema contribution seam (Phase 1.5 §5.1.2 BPV-D01 remediation).
///
/// A schema module owns the binding between a domain's CLR event types and the
/// runtime <see cref="EventSchemaRegistry"/>. Implementations live in the runtime
/// layer (which may legitimately reference both domain events and
/// shared-contract schema records); host composition modules MUST NOT type
/// domain events directly. Host bootstraps reach schema modules indirectly via
/// <see cref="DomainSchemaCatalog"/>.
/// </summary>
public interface ISchemaModule
{
    void Register(ISchemaSink sink);
}

/// <summary>
/// Minimal sink surface a schema module needs from <see cref="EventSchemaRegistry"/>.
/// Kept narrow on purpose so future seams (test doubles, validators, drift
/// detectors) can implement it without depending on the concrete registry.
/// </summary>
public interface ISchemaSink
{
    void RegisterSchema(string eventTypeName, EventVersion version, System.Type storedEventType, System.Type inboundEventType);
    void RegisterPayloadMapper(string eventTypeName, System.Func<object, object> mapper);
}
