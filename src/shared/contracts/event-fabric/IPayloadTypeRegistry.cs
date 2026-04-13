namespace Whycespace.Shared.Contracts.EventFabric;

/// <summary>
/// Registry of CLR types that may appear as opaque <c>object?</c> payloads or
/// step outputs on workflow lifecycle events. Bootstrap modules register their
/// payload types here so the event store replay path can rehydrate them from
/// JSON back into typed CLR objects.
///
/// The registry is event-agnostic and domain-agnostic — it is keyed on type
/// name strings only. This keeps the runtime/event-fabric deserialization seam
/// compliant with runtime.guard rule 11.R-DOM-01 (no concrete domain symbols
/// in src/runtime/** or src/platform/host/**).
///
/// Write side (factories): use <see cref="TryGetName"/> for back-compat —
/// unregistered types resolve to null PayloadType and round-trip as today's
/// untyped JsonElement on Postgres replay.
///
/// Read side (deserializer): use <see cref="Resolve"/> — strict, throws on
/// unknown type names so a PayloadType written but never registered fails
/// fast at replay rather than silently corrupting state.
/// </summary>
public interface IPayloadTypeRegistry
{
    /// <summary>Registers a CLR type so it can be rehydrated on replay.</summary>
    void Register(System.Type type);

    /// <summary>Generic convenience overload of <see cref="Register(System.Type)"/>.</summary>
    void Register<T>();

    /// <summary>
    /// Returns the canonical name written into the event when the type is
    /// registered. Returns false (and a null name) if the type is not
    /// registered — callers should treat this as "skip the type metadata"
    /// for back-compat.
    /// </summary>
    bool TryGetName(System.Type type, out string? name);

    /// <summary>Strict resolution. Throws if <paramref name="typeName"/> is unknown.</summary>
    System.Type Resolve(string typeName);
}
