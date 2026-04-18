using System.Text.Json;
using Whycespace.Domain.EconomicSystem.Risk.Exposure;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.EventFabric.DomainSchemas;
using Whycespace.Shared.Contracts.Events.Economic.Risk.Exposure;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Integration.Runtime.EventFabric;

/// <summary>
/// Phase 6 Final Patch — pins that <c>ExposureBreachedEvent</c> is
/// registered end-to-end in the economic schema module:
///
///   1. Stored + inbound schema types are resolvable via
///      <see cref="EventSchemaRegistry"/>.
///   2. <c>MapPayload</c> produces the on-wire
///      <see cref="RiskExposureBreachedEventSchema"/> shape carrying
///      the (ExposureId, TotalExposure, Threshold, Currency,
///      DetectedAt) tuple.
///   3. The produced JSON round-trips back through
///      <see cref="EventDeserializer.DeserializeInbound"/> with every
///      field preserved verbatim.
///
/// Without these registrations the <c>RiskExposureEnforcementWorker</c>
/// would fail fast with "EventSchemaRegistry has no InboundEventType
/// registered for 'ExposureBreachedEvent'", DLQ the message, and the
/// risk → enforcement loop would never fire.
/// </summary>
public sealed class ExposureBreachedSchemaRegistrationTests
{
    private static readonly TestIdGenerator IdGen = new();

    private static Whycespace.Runtime.EventFabric.EventSchemaRegistry NewRegistry()
    {
        var registry = new Whycespace.Runtime.EventFabric.EventSchemaRegistry();
        DomainSchemaCatalog.RegisterEconomic(registry);
        return registry;
    }

    [Fact]
    public void Registry_Resolves_ExposureBreachedEvent()
    {
        var registry = NewRegistry();

        var entry = registry.Resolve("ExposureBreachedEvent");

        Assert.NotNull(entry);
        Assert.Equal(typeof(ExposureBreachedEvent), entry.StoredEventType);
        Assert.Equal(typeof(RiskExposureBreachedEventSchema), entry.InboundEventType);
    }

    [Fact]
    public void MapPayload_Produces_CanonicalOnWireShape()
    {
        var registry = NewRegistry();

        var domainEvent = new ExposureBreachedEvent(
            new ExposureId(IdGen.Generate("schema:exposure")),
            new Amount(1_000m),
            new Amount(500m),
            new Currency("USD"),
            new Timestamp(new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero)));

        var mapped = registry.MapPayload("ExposureBreachedEvent", domainEvent);

        var schema = Assert.IsType<RiskExposureBreachedEventSchema>(mapped);
        Assert.Equal(domainEvent.ExposureId.Value, schema.AggregateId);
        Assert.Equal(1_000m, schema.TotalExposure);
        Assert.Equal(500m, schema.Threshold);
        Assert.Equal("USD", schema.Currency);
        Assert.Equal(domainEvent.DetectedAt.Value, schema.DetectedAt);
    }

    [Fact]
    public void DeserializeInbound_RoundTripsEvery_OnWireField()
    {
        var registry = NewRegistry();
        var deserializer = new EventDeserializer(registry);

        var exposureId = IdGen.Generate("schema:roundtrip:exposure");
        var onWire = new RiskExposureBreachedEventSchema(
            exposureId,
            TotalExposure: 2_500m,
            Threshold: 1_000m,
            Currency: "EUR",
            DetectedAt: new DateTimeOffset(2026, 4, 17, 12, 0, 0, TimeSpan.Zero));

        var json = JsonSerializer.Serialize(onWire);
        var rehydrated = deserializer.DeserializeInbound("ExposureBreachedEvent", json);

        var schema = Assert.IsType<RiskExposureBreachedEventSchema>(rehydrated);
        Assert.Equal(exposureId, schema.AggregateId);
        Assert.Equal(2_500m, schema.TotalExposure);
        Assert.Equal(1_000m, schema.Threshold);
        Assert.Equal("EUR", schema.Currency);
        Assert.Equal(onWire.DetectedAt, schema.DetectedAt);
    }
}
