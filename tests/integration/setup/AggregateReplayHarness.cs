using System.Text.Json;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Runtime.EventFabric;

namespace Whycespace.Tests.Integration.Setup;

/// <summary>
/// β #2 — generic replay-roundtrip harness for INV-REPLAY-LOSSLESS-VALUEOBJECT-01.
///
/// Verifies that for any aggregate which:
///   1. Was created via its public factory,
///   2. Persisted its seed event(s) through the schema-mapping layer
///      (mirroring the EventStore write path),
/// the events round-trip losslessly through:
///   payload-map → JSON serialize → JSON deserialize via EventDeserializer →
///   LoadFromHistory → assertions on the reconstructed state.
///
/// Per-aggregate test files supply the factory output and a state-comparison
/// callback. The harness wires the schema registry exactly as the host does
/// (via DomainSchemaCatalog.RegisterEconomic and the operational catalog,
/// per the per-classification need).
///
/// Failure mode this catches: any value-object constructor parameter on a
/// domain event that the EventDeserializer.StoredOptions cannot reconstruct
/// — which silently produces default(T) and corrupts the replayed aggregate.
/// </summary>
public static class AggregateReplayHarness
{
    /// <summary>
    /// Round-trips the supplied aggregate's <see cref="AggregateRoot.DomainEvents"/>
    /// through the same JSON path used by <c>PostgresEventStoreAdapter</c> ↔
    /// <c>EventDeserializer.DeserializeStored</c> ↔ <c>LoadFromHistory</c>,
    /// then invokes <paramref name="assertions"/> against the freshly replayed
    /// instance so the caller can compare any value-object fields it cares
    /// about. Throws on any mismatch.
    /// </summary>
    /// <typeparam name="TAggregate">aggregate type (must have a private parameterless ctor)</typeparam>
    /// <param name="seed">the aggregate produced by the factory under test</param>
    /// <param name="registry">a schema registry pre-populated with the events this aggregate emits</param>
    /// <param name="assertions">callback invoked with the aggregate replayed from history</param>
    public static void VerifyRoundTrip<TAggregate>(
        TAggregate seed,
        Whycespace.Runtime.EventFabric.EventSchemaRegistry registry,
        Action<TAggregate> assertions)
        where TAggregate : AggregateRoot
    {
        if (seed.DomainEvents.Count == 0)
            throw new InvalidOperationException(
                "Replay harness requires at least one domain event on the seed aggregate.");

        var deserializer = new EventDeserializer(registry);

        // Mirror PostgresEventStoreAdapter: persist the schema-mapped form.
        var rehydrated = new List<object>(seed.DomainEvents.Count);
        foreach (var domainEvent in seed.DomainEvents)
        {
            var eventTypeName = domainEvent.GetType().Name;
            var mapped = registry.MapPayload(eventTypeName, domainEvent);
            var jsonb = JsonSerializer.Serialize(mapped, mapped.GetType());
            rehydrated.Add(deserializer.DeserializeStored(eventTypeName, jsonb));
        }

        // Replay onto a fresh aggregate (mirrors LoadFromHistory in the runtime).
        var replayed = (TAggregate)Activator.CreateInstance(typeof(TAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(rehydrated);

        assertions(replayed);
    }
}
