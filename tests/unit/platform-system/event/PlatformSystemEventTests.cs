using Whycespace.Domain.PlatformSystem.Event.EventDefinition;
using Whycespace.Domain.PlatformSystem.Event.EventMetadata;
using Whycespace.Domain.PlatformSystem.Event.EventSchema;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Tests.Unit.PlatformSystem.Event;

/// <summary>
/// E12 — Domain tests for the event context: event-definition, event-metadata, event-schema.
/// Covers topics 6 (aggregate design), 26 (test certification) of platform-system.md.
/// </summary>
public sealed class PlatformSystemEventTests
{
    private static readonly Guid _id1 = new("cccccccc-0000-0000-0000-000000000001");
    private static readonly Guid _id2 = new("cccccccc-0000-0000-0000-000000000002");
    private static readonly Timestamp _ts = new(new DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero));
    private static readonly DomainRoute _route = new("platform-system", "event", "event-definition");

    // -------------------------------------------------------------------------
    // EventDefinitionAggregate
    // -------------------------------------------------------------------------

    [Fact]
    public void EventDefinition_Define_ValidInputs_SetsState()
    {
        var agg = EventDefinitionAggregate.Define(
            new EventDefinitionId(_id1),
            new EventTypeName("VaultFunded"),
            new EventVersion("1.0"),
            "schema-evt-001",
            _route,
            _ts);

        Assert.Equal(_id1, agg.EventDefinitionId.Value);
        Assert.Equal("VaultFunded", agg.TypeName.Value);
        Assert.Equal("1.0", agg.Version.Value);
        Assert.Equal("schema-evt-001", agg.SchemaId);
        Assert.Equal(EventDefinitionStatus.Active, agg.Status);
    }

    [Fact]
    public void EventDefinition_Define_RaisesEventDefinedEvent()
    {
        var agg = EventDefinitionAggregate.Define(
            new EventDefinitionId(_id1),
            new EventTypeName("AccountOpened"),
            new EventVersion("2.0"),
            "schema-001",
            _route,
            _ts);

        Assert.Single(agg.DomainEvents);
        Assert.IsType<EventDefinedEvent>(agg.DomainEvents[0]);
    }

    [Fact]
    public void EventDefinition_Define_EmptySchemaId_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            EventDefinitionAggregate.Define(
                new EventDefinitionId(_id1),
                new EventTypeName("AccountOpened"),
                new EventVersion("1.0"),
                "",
                _route,
                _ts));
    }

    [Fact]
    public void EventDefinition_Define_InvalidRoute_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            EventDefinitionAggregate.Define(
                new EventDefinitionId(_id1),
                new EventTypeName("AccountOpened"),
                new EventVersion("1.0"),
                "schema-001",
                new DomainRoute("", "", ""),
                _ts));
    }

    [Fact]
    public void EventDefinition_Deprecate_ChangesStatus()
    {
        var agg = EventDefinitionAggregate.Define(
            new EventDefinitionId(_id1),
            new EventTypeName("AccountOpened"),
            new EventVersion("1.0"),
            "schema-001",
            _route,
            _ts);

        agg.Deprecate(_ts);

        Assert.Equal(EventDefinitionStatus.Deprecated, agg.Status);
        Assert.Equal(2, agg.DomainEvents.Count);
    }

    [Fact]
    public void EventDefinition_Deprecate_AlreadyDeprecated_Throws()
    {
        var agg = EventDefinitionAggregate.Define(
            new EventDefinitionId(_id1),
            new EventTypeName("AccountOpened"),
            new EventVersion("1.0"),
            "schema-001",
            _route,
            _ts);
        agg.Deprecate(_ts);

        Assert.Throws<DomainInvariantViolationException>(() => agg.Deprecate(_ts));
    }

    [Fact]
    public void EventVersion_EmptyString_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() => new EventVersion(""));
    }

    // -------------------------------------------------------------------------
    // EventSchemaAggregate (shadowed Version property)
    // -------------------------------------------------------------------------

    [Fact]
    public void EventSchema_Register_ValidInputs_SetsState()
    {
        var agg = EventSchemaAggregate.Register(
            new EventSchemaId(_id1),
            new EventSchemaName("VaultFundedEvent"),
            new EventSchemaVersion("v1"),
            CompatibilityMode.Backward,
            _ts);

        Assert.Equal(_id1, agg.EventSchemaId.Value);
        Assert.Equal("VaultFundedEvent", agg.Name.Value);
        Assert.Equal("v1", agg.Version.Value);
        Assert.Equal(CompatibilityMode.Backward, agg.CompatibilityMode);
        Assert.False(agg.IsDeprecated);
    }

    [Fact]
    public void EventSchema_Register_RaisesRegisteredEvent()
    {
        var agg = EventSchemaAggregate.Register(
            new EventSchemaId(_id1),
            new EventSchemaName("AccountOpenedEvent"),
            new EventSchemaVersion("v2"),
            CompatibilityMode.Full,
            _ts);

        Assert.Single(agg.DomainEvents);
        Assert.IsType<EventSchemaRegisteredEvent>(agg.DomainEvents[0]);
    }

    [Fact]
    public void EventSchema_Deprecate_SetsIsDeprecated()
    {
        var agg = EventSchemaAggregate.Register(
            new EventSchemaId(_id1),
            new EventSchemaName("VaultFundedEvent"),
            new EventSchemaVersion("v1"),
            CompatibilityMode.None,
            _ts);

        agg.Deprecate(_ts);

        Assert.True(agg.IsDeprecated);
        Assert.Equal(2, agg.DomainEvents.Count);
        Assert.IsType<EventSchemaDeprecatedEvent>(agg.DomainEvents[1]);
    }

    [Fact]
    public void EventSchema_Deprecate_AlreadyDeprecated_Throws()
    {
        var agg = EventSchemaAggregate.Register(
            new EventSchemaId(_id1),
            new EventSchemaName("VaultFundedEvent"),
            new EventSchemaVersion("v1"),
            CompatibilityMode.Forward,
            _ts);
        agg.Deprecate(_ts);

        Assert.Throws<DomainInvariantViolationException>(() => agg.Deprecate(_ts));
    }

    [Fact]
    public void EventSchema_ShadowedVersionProperty_ReturnsSchemaVersion_NotBaseVersion()
    {
        var agg = EventSchemaAggregate.Register(
            new EventSchemaId(_id1),
            new EventSchemaName("VaultFundedEvent"),
            new EventSchemaVersion("v3"),
            CompatibilityMode.Backward,
            _ts);

        // The domain Version property is EventSchemaVersion (not the base int counter).
        Assert.Equal("v3", agg.Version.Value);
        Assert.IsType<EventSchemaVersion>(agg.Version);
    }

    [Fact]
    public void CompatibilityMode_StaticValues_AreDistinct()
    {
        Assert.NotEqual(CompatibilityMode.Backward, CompatibilityMode.Forward);
        Assert.NotEqual(CompatibilityMode.Forward, CompatibilityMode.Full);
        Assert.NotEqual(CompatibilityMode.Full, CompatibilityMode.None);
    }

    // -------------------------------------------------------------------------
    // EventMetadataAggregate
    // -------------------------------------------------------------------------

    [Fact]
    public void EventMetadata_Attach_ValidInputs_SetsState()
    {
        var agg = EventMetadataAggregate.Attach(
            new EventMetadataId(_id1),
            new EventEnvelopeRef(_id2),
            new ExecutionHash("abc123def456"),
            new PolicyDecisionHash("poldec-hash"),
            new EventActorId("actor-001"),
            new EventTraceId("trace-aaa"),
            new EventSpanId("span-001"),
            _ts);

        Assert.Equal(_id1, agg.EventMetadataId.Value);
        Assert.Equal(_id2, agg.EnvelopeRef.Value);
        Assert.Equal("abc123def456", agg.ExecutionHash.Value);
        Assert.Equal("actor-001", agg.ActorId.Value);
    }

    [Fact]
    public void EventMetadata_Attach_RaisesAttachedEvent()
    {
        var agg = EventMetadataAggregate.Attach(
            new EventMetadataId(_id1),
            new EventEnvelopeRef(_id2),
            new ExecutionHash("hash-x"),
            new PolicyDecisionHash("pol-hash"),
            new EventActorId("actor-001"),
            new EventTraceId("trace-bbb"),
            new EventSpanId("span-002"),
            _ts);

        Assert.Single(agg.DomainEvents);
        Assert.IsType<EventMetadataAttachedEvent>(agg.DomainEvents[0]);
    }

    [Fact]
    public void ExecutionHash_ReplaySentinel_IsDetected()
    {
        var hash = new ExecutionHash(ExecutionHash.ReplaySentinel);
        Assert.True(hash.IsReplay);
    }

    [Fact]
    public void ExecutionHash_NonReplay_IsNotReplay()
    {
        var hash = new ExecutionHash("sha256:abc123");
        Assert.False(hash.IsReplay);
    }

    [Fact]
    public void PolicyDecisionHash_ReplaySentinel_IsDetected()
    {
        var hash = new PolicyDecisionHash(PolicyDecisionHash.ReplaySentinel);
        Assert.True(hash.IsReplay);
    }

    [Fact]
    public void EventEnvelopeRef_EmptyGuid_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() => new EventEnvelopeRef(Guid.Empty));
    }
}
