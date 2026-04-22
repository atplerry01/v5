using Whycespace.Domain.PlatformSystem.Command.CommandDefinition;
using Whycespace.Domain.PlatformSystem.Command.CommandMetadata;
using Whycespace.Domain.PlatformSystem.Command.CommandRouting;
using Whycespace.Domain.PlatformSystem.Envelope.Header;
using Whycespace.Domain.PlatformSystem.Envelope.Metadata;
using Whycespace.Domain.PlatformSystem.Envelope.Payload;
using Whycespace.Domain.PlatformSystem.Event.EventDefinition;
using Whycespace.Domain.PlatformSystem.Event.EventMetadata;
using Whycespace.Domain.PlatformSystem.Event.EventSchema;
using Whycespace.Domain.PlatformSystem.Event.EventStream;
using Whycespace.Domain.PlatformSystem.Routing.DispatchRule;
using Whycespace.Domain.PlatformSystem.Routing.RouteDefinition;
using Whycespace.Domain.PlatformSystem.Routing.RouteDescriptor;
using Whycespace.Domain.PlatformSystem.Routing.RouteResolution;
using Whycespace.Domain.PlatformSystem.Schema.Contract;
using Whycespace.Domain.PlatformSystem.Schema.SchemaDefinition;
using Whycespace.Domain.PlatformSystem.Schema.Serialization;
using Whycespace.Domain.PlatformSystem.Schema.Versioning;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Tests.Unit.PlatformSystem;

/// <summary>
/// E12 — Cross-cutting regression tests for platform-system: replay determinism,
/// AggregateRoot.Version invariant, lifecycle guards, and type safety across all 5 contexts.
/// Covers topics 5 (lifecycle), 6 (aggregate design), 26 (test certification) of platform-system.md.
/// </summary>
public sealed class PlatformSystemRegressionTests
{
    private static readonly Guid _id1 = new("ffffffff-0000-0000-0000-000000000001");
    private static readonly Guid _id2 = new("ffffffff-0000-0000-0000-000000000002");
    private static readonly Guid _id3 = new("ffffffff-0000-0000-0000-000000000003");
    private static readonly Timestamp _ts = new(new DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero));
    private static readonly DomainRoute _src = new("platform-system", "command", "command-definition");
    private static readonly DomainRoute _dst = new("economic-system", "capital", "vault");

    // -------------------------------------------------------------------------
    // Replay Determinism — same inputs → equal state across contexts
    // -------------------------------------------------------------------------

    [Fact]
    public void Replay_CommandContext_SameInputs_ProduceEqualState()
    {
        var agg1 = CommandDefinitionAggregate.Define(
            new CommandDefinitionId(_id1), new CommandTypeName("OpenAccount"),
            new CommandVersion(1), "schema-cmd-001", _src, _ts);
        var agg2 = CommandDefinitionAggregate.Define(
            new CommandDefinitionId(_id1), new CommandTypeName("OpenAccount"),
            new CommandVersion(1), "schema-cmd-001", _src, _ts);

        Assert.Equal(agg1.CommandDefinitionId.Value, agg2.CommandDefinitionId.Value);
        Assert.Equal(agg1.TypeName.Value, agg2.TypeName.Value);
        Assert.Equal(agg1.SchemaId, agg2.SchemaId);
        Assert.Equal(agg1.Status, agg2.Status);
    }

    [Fact]
    public void Replay_EnvelopeContext_SameInputs_ProduceEqualState()
    {
        IReadOnlyList<string> fields = ["MessageId", "ContentType", "SourceAddress"];
        var agg1 = HeaderSchemaAggregate.Register(
            new HeaderSchemaId(_id1), new HeaderKind("command"), 1, fields, _ts);
        var agg2 = HeaderSchemaAggregate.Register(
            new HeaderSchemaId(_id1), new HeaderKind("command"), 1, fields, _ts);

        Assert.Equal(agg1.SchemaVersion, agg2.SchemaVersion);
        Assert.Equal(agg1.HeaderKind.Value, agg2.HeaderKind.Value);
        Assert.Equal(agg1.Status, agg2.Status);
    }

    [Fact]
    public void Replay_EventContext_SameInputs_ProduceEqualState()
    {
        var agg1 = EventSchemaAggregate.Register(
            new EventSchemaId(_id1), new EventSchemaName("VaultFunded"),
            new EventSchemaVersion("v1"), CompatibilityMode.Backward, _ts);
        var agg2 = EventSchemaAggregate.Register(
            new EventSchemaId(_id1), new EventSchemaName("VaultFunded"),
            new EventSchemaVersion("v1"), CompatibilityMode.Backward, _ts);

        Assert.Equal(agg1.Name.Value, agg2.Name.Value);
        Assert.Equal(agg1.Version.Value, agg2.Version.Value);
        Assert.Equal(agg1.CompatibilityMode, agg2.CompatibilityMode);
        Assert.Equal(agg1.IsDeprecated, agg2.IsDeprecated);
    }

    [Fact]
    public void Replay_RoutingContext_SameInputs_ProduceEqualState()
    {
        var cond = new DispatchCondition(DispatchConditionType.AlwaysMatch, "");
        var agg1 = DispatchRuleAggregate.Register(
            new DispatchRuleId(_id1), "Route-All", _id2, cond, 10, _ts);
        var agg2 = DispatchRuleAggregate.Register(
            new DispatchRuleId(_id1), "Route-All", _id2, cond, 10, _ts);

        Assert.Equal(agg1.RuleName, agg2.RuleName);
        Assert.Equal(agg1.Priority, agg2.Priority);
        Assert.Equal(agg1.RouteRef, agg2.RouteRef);
        Assert.Equal(agg1.Status, agg2.Status);
    }

    [Fact]
    public void Replay_SchemaContext_SameInputs_ProduceEqualState()
    {
        var agg1 = ContractAggregate.Register(
            new ContractId(_id1), "VaultFundedContract", _src, _id2, 1, _ts);
        var agg2 = ContractAggregate.Register(
            new ContractId(_id1), "VaultFundedContract", _src, _id2, 1, _ts);

        Assert.Equal(agg1.ContractName, agg2.ContractName);
        Assert.Equal(agg1.SchemaRef, agg2.SchemaRef);
        Assert.Equal(agg1.Status, agg2.Status);
        Assert.Empty(agg1.SubscriberConstraints);
        Assert.Empty(agg2.SubscriberConstraints);
    }

    // -------------------------------------------------------------------------
    // AggregateRoot.Version invariant — stays -1 after factory (RaiseDomainEvent
    // does NOT increment Version; only LoadFromHistory does)
    // -------------------------------------------------------------------------

    [Fact]
    public void AggregateRoot_BaseVersion_RemainsNegativeOne_AfterCommandDefinitionFactory()
    {
        var agg = CommandDefinitionAggregate.Define(
            new CommandDefinitionId(_id1), new CommandTypeName("OpenAccount"),
            new CommandVersion(1), "schema-001", _src, _ts);

        Assert.Equal(-1, ((AggregateRoot)agg).Version);
        Assert.NotEmpty(agg.DomainEvents);
    }

    [Fact]
    public void AggregateRoot_BaseVersion_RemainsNegativeOne_EventSchemaFactory_ShadowedVersion()
    {
        // EventSchemaAggregate.Version (EventSchemaVersion) shadows AggregateRoot.Version (int).
        // Base Version must stay -1 even though the domain Version is typed as EventSchemaVersion.
        var agg = EventSchemaAggregate.Register(
            new EventSchemaId(_id1), new EventSchemaName("VaultFunded"),
            new EventSchemaVersion("v1"), CompatibilityMode.Backward, _ts);

        Assert.Equal(-1, ((AggregateRoot)agg).Version);
        Assert.Equal("v1", agg.Version.Value);
        Assert.IsType<EventSchemaVersion>(agg.Version);
    }

    [Fact]
    public void AggregateRoot_BaseVersion_RemainsNegativeOne_EventDefinitionFactory_ShadowedVersion()
    {
        // EventDefinitionAggregate.Version (EventVersion) shadows AggregateRoot.Version (int).
        var agg = EventDefinitionAggregate.Define(
            new EventDefinitionId(_id1), new EventTypeName("AccountOpened"),
            new EventVersion("2.0"), "schema-001", _src, _ts);

        Assert.Equal(-1, ((AggregateRoot)agg).Version);
        Assert.Equal("2.0", agg.Version.Value);
        Assert.IsType<EventVersion>(agg.Version);
    }

    // -------------------------------------------------------------------------
    // All-context event emission — every factory raises ≥ 1 domain event
    // -------------------------------------------------------------------------

    [Fact]
    public void CommandContext_AllFactories_EmitDomainEvents()
    {
        var cmdDef = CommandDefinitionAggregate.Define(
            new CommandDefinitionId(_id1), new CommandTypeName("OpenAccount"),
            new CommandVersion(1), "schema-001", _src, _ts);

        var cmdMeta = CommandMetadataAggregate.Attach(
            new CommandMetadataId(_id1), _id2,
            new MetadataActorId("actor-001"), new MetadataTraceId("trace-abc"),
            new MetadataSpanId("span-001"), new PolicyContextRef(null, null),
            new TrustScore(75), _ts);

        var cmdRoute = CommandRoutingRuleAggregate.Register(
            new CommandRoutingRuleId(_id1), new CommandTypeRef(_id2), _dst, _ts);

        Assert.NotEmpty(cmdDef.DomainEvents);
        Assert.NotEmpty(cmdMeta.DomainEvents);
        Assert.NotEmpty(cmdRoute.DomainEvents);
    }

    [Fact]
    public void EnvelopeContext_AllFactories_EmitDomainEvents()
    {
        IReadOnlyList<string> hdrFields = ["MessageId", "ContentType", "SourceAddress"];
        var header = HeaderSchemaAggregate.Register(
            new HeaderSchemaId(_id1), new HeaderKind("event"), 1, hdrFields, _ts);

        IReadOnlyList<string> metaFields = ["CorrelationId", "CausationId", "IssuedAt", "MessageVersion"];
        var meta = MessageMetadataSchemaAggregate.Register(
            new MetadataSchemaId(_id1), 1, metaFields, [], _ts);

        var payload = PayloadSchemaAggregate.Register(
            new PayloadSchemaId(_id1), "vault.funded", PayloadEncoding.Json,
            null, 1, null, _ts);

        Assert.NotEmpty(header.DomainEvents);
        Assert.NotEmpty(meta.DomainEvents);
        Assert.NotEmpty(payload.DomainEvents);
    }

    [Fact]
    public void EventContext_AllFactories_EmitDomainEvents()
    {
        var evtDef = EventDefinitionAggregate.Define(
            new EventDefinitionId(_id1), new EventTypeName("VaultFunded"),
            new EventVersion("1.0"), "schema-evt-001", _src, _ts);

        var evtSchema = EventSchemaAggregate.Register(
            new EventSchemaId(_id1), new EventSchemaName("VaultFundedEvent"),
            new EventSchemaVersion("v1"), CompatibilityMode.Backward, _ts);

        var evtMeta = EventMetadataAggregate.Attach(
            new EventMetadataId(_id1), new EventEnvelopeRef(_id2),
            new ExecutionHash("sha256:abc"), new PolicyDecisionHash("poldec-hash"),
            new EventActorId("actor-001"), new EventTraceId("trace-aaa"),
            new EventSpanId("span-001"), _ts);

        var evtStream = EventStreamAggregate.Declare(
            new EventStreamId(_id1), _src,
            ["VaultFunded", "AccountOpened"], OrderingGuarantee.Ordered, _ts);

        Assert.NotEmpty(evtDef.DomainEvents);
        Assert.NotEmpty(evtSchema.DomainEvents);
        Assert.NotEmpty(evtMeta.DomainEvents);
        Assert.NotEmpty(evtStream.DomainEvents);
    }

    [Fact]
    public void RoutingContext_AllFactories_EmitDomainEvents()
    {
        var cond = new DispatchCondition(DispatchConditionType.AlwaysMatch, "");
        var dispatch = DispatchRuleAggregate.Register(
            new DispatchRuleId(_id1), "Route-All", _id2, cond, 0, _ts);

        var routeDef = RouteDefinitionAggregate.Register(
            new RouteDefinitionId(_id1), "VaultRoute", _src, _dst,
            TransportHint.Kafka, 1, _ts);

        var routeDesc = RouteDescriptorAggregate.Register(
            new RouteDescriptorId(_id1), _src, _dst, "kafka", 1, _ts);

        var resolution = RouteResolutionAggregate.Resolve(
            new ResolutionId(_id1), _src, "VaultFunded", _id2,
            ResolutionStrategy.ExactMatch, [_id3], _ts);

        Assert.NotEmpty(dispatch.DomainEvents);
        Assert.NotEmpty(routeDef.DomainEvents);
        Assert.NotEmpty(routeDesc.DomainEvents);
        Assert.NotEmpty(resolution.DomainEvents);
    }

    [Fact]
    public void SchemaContext_AllFactories_EmitDomainEvents()
    {
        var contract = ContractAggregate.Register(
            new ContractId(_id1), "VaultFundedContract", _src, _id2, 1, _ts);

        var schemaDef = SchemaDefinitionAggregate.Draft(
            new SchemaDefinitionId(_id1), new SchemaName("VaultFundedEventSchema"),
            1, [new FieldDescriptor("aggregateId", FieldType.String, true, null, null)],
            SchemaCompatibilityMode.Backward, _ts);

        var serialFmt = SerializationFormatAggregate.Register(
            new SerializationFormatId(_id1), "json-v1", SerializationEncoding.Json,
            null, [], RoundTripGuarantee.Lossless, 1, _ts);

        var versionRule = VersioningRuleAggregate.Register(
            new VersioningRuleId(_id1), _id2, 1, 2,
            EvolutionClass.NonBreaking,
            [new SchemaChange(SchemaChangeType.FieldAdded, "newField", ChangeImpact.Safe)],
            _ts);

        Assert.NotEmpty(contract.DomainEvents);
        Assert.NotEmpty(schemaDef.DomainEvents);
        Assert.NotEmpty(serialFmt.DomainEvents);
        Assert.NotEmpty(versionRule.DomainEvents);
    }
}
