using Whycespace.Domain.PlatformSystem.Command.CommandDefinition;
using Whycespace.Domain.PlatformSystem.Command.CommandMetadata;
using Whycespace.Domain.PlatformSystem.Command.CommandRouting;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Tests.Unit.PlatformSystem.Command;

/// <summary>
/// E12 — Domain tests for the command context: command-definition, command-metadata, command-routing.
/// Covers topics 5 (lifecycle), 6 (aggregate design), 26 (test certification) of control-system.md.
/// </summary>
public sealed class PlatformSystemCommandTests
{
    private static readonly Guid _id1 = new("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid _id2 = new("aaaaaaaa-0000-0000-0000-000000000002");
    private static readonly Guid _id3 = new("aaaaaaaa-0000-0000-0000-000000000003");
    private static readonly Timestamp _ts = new(new DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero));
    private static readonly DomainRoute _route = new("platform-system", "command", "command-definition");
    private static readonly DomainRoute _handler = new("platform-system", "command", "command-routing");

    // -------------------------------------------------------------------------
    // CommandDefinitionAggregate — construction
    // -------------------------------------------------------------------------

    [Fact]
    public void CommandDefinition_Define_ValidInputs_SetsState()
    {
        var agg = CommandDefinitionAggregate.Define(
            new CommandDefinitionId(_id1),
            new CommandTypeName("DefineVault"),
            new CommandVersion(1),
            "schema-001",
            _route,
            _ts);

        Assert.Equal(_id1, agg.CommandDefinitionId.Value);
        Assert.Equal("DefineVault", agg.TypeName.Value);
        Assert.Equal(1, agg.TypeVersion.Value);
        Assert.Equal("schema-001", agg.SchemaId);
        Assert.Equal(CommandDefinitionStatus.Active, agg.Status);
    }

    [Fact]
    public void CommandDefinition_Define_RaisesCommandDefinedEvent()
    {
        var agg = CommandDefinitionAggregate.Define(
            new CommandDefinitionId(_id1),
            new CommandTypeName("CreateAccount"),
            new CommandVersion(2),
            "schema-002",
            _route,
            _ts);

        Assert.Single(agg.DomainEvents);
        Assert.IsType<CommandDefinedEvent>(agg.DomainEvents[0]);
    }

    [Fact]
    public void CommandDefinition_Define_EmptySchemaId_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            CommandDefinitionAggregate.Define(
                new CommandDefinitionId(_id1),
                new CommandTypeName("CreateAccount"),
                new CommandVersion(1),
                "",
                _route,
                _ts));
    }

    [Fact]
    public void CommandDefinition_Define_InvalidRoute_Throws()
    {
        var invalid = new DomainRoute("", "", "");
        Assert.Throws<DomainInvariantViolationException>(() =>
            CommandDefinitionAggregate.Define(
                new CommandDefinitionId(_id1),
                new CommandTypeName("CreateAccount"),
                new CommandVersion(1),
                "schema-001",
                invalid,
                _ts));
    }

    [Fact]
    public void CommandDefinition_Deprecate_ChangesStatus()
    {
        var agg = CommandDefinitionAggregate.Define(
            new CommandDefinitionId(_id1),
            new CommandTypeName("CreateAccount"),
            new CommandVersion(1),
            "schema-001",
            _route,
            _ts);

        agg.Deprecate(_ts);

        Assert.Equal(CommandDefinitionStatus.Deprecated, agg.Status);
        Assert.Equal(2, agg.DomainEvents.Count);
        Assert.IsType<CommandDeprecatedEvent>(agg.DomainEvents[1]);
    }

    [Fact]
    public void CommandDefinition_Deprecate_AlreadyDeprecated_Throws()
    {
        var agg = CommandDefinitionAggregate.Define(
            new CommandDefinitionId(_id1),
            new CommandTypeName("CreateAccount"),
            new CommandVersion(1),
            "schema-001",
            _route,
            _ts);
        agg.Deprecate(_ts);

        Assert.Throws<DomainInvariantViolationException>(() => agg.Deprecate(_ts));
    }

    // -------------------------------------------------------------------------
    // CommandVersion VO
    // -------------------------------------------------------------------------

    [Fact]
    public void CommandVersion_ValidValue_Accepted()
    {
        var v = new CommandVersion(1);
        Assert.Equal(1, v.Value);
    }

    [Fact]
    public void CommandVersion_Zero_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() => new CommandVersion(0));
    }

    [Fact]
    public void CommandVersion_Negative_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() => new CommandVersion(-1));
    }

    [Fact]
    public void CommandTypeName_EmptyString_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() => new CommandTypeName(""));
    }

    // -------------------------------------------------------------------------
    // CommandMetadataAggregate
    // -------------------------------------------------------------------------

    [Fact]
    public void CommandMetadata_Attach_ValidInputs_SetsState()
    {
        var agg = CommandMetadataAggregate.Attach(
            new CommandMetadataId(_id1),
            _id2,
            new MetadataActorId("actor-001"),
            new MetadataTraceId("trace-abc"),
            new MetadataSpanId("span-001"),
            new PolicyContextRef("pol-1", "1.0"),
            new TrustScore(85),
            _ts);

        Assert.Equal(_id1, agg.CommandMetadataId.Value);
        Assert.Equal(_id2, agg.EnvelopeRef);
        Assert.Equal(85, agg.TrustScore.Value);
    }

    [Fact]
    public void CommandMetadata_Attach_RaisesAttachedEvent()
    {
        var agg = CommandMetadataAggregate.Attach(
            new CommandMetadataId(_id1),
            _id2,
            new MetadataActorId("actor-001"),
            new MetadataTraceId("trace-abc"),
            new MetadataSpanId("span-001"),
            new PolicyContextRef(null, null),
            new TrustScore(50),
            _ts);

        Assert.Single(agg.DomainEvents);
        Assert.IsType<CommandMetadataAttachedEvent>(agg.DomainEvents[0]);
    }

    [Fact]
    public void CommandMetadata_Attach_EmptyEnvelopeRef_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            CommandMetadataAggregate.Attach(
                new CommandMetadataId(_id1),
                Guid.Empty,
                new MetadataActorId("actor"),
                new MetadataTraceId("trace"),
                new MetadataSpanId("span"),
                new PolicyContextRef(null, null),
                new TrustScore(0),
                _ts));
    }

    [Fact]
    public void TrustScore_OutOfRange_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() => new TrustScore(101));
        Assert.Throws<DomainInvariantViolationException>(() => new TrustScore(-1));
    }

    [Fact]
    public void TrustScore_Boundary_Zero_And_Hundred_Accepted()
    {
        Assert.Equal(0, new TrustScore(0).Value);
        Assert.Equal(100, new TrustScore(100).Value);
    }

    // -------------------------------------------------------------------------
    // CommandRoutingRuleAggregate
    // -------------------------------------------------------------------------

    [Fact]
    public void CommandRoutingRule_Register_ValidInputs_SetsState()
    {
        var agg = CommandRoutingRuleAggregate.Register(
            new CommandRoutingRuleId(_id1),
            new CommandTypeRef(_id2),
            _handler,
            _ts);

        Assert.Equal(_id1, agg.CommandRoutingRuleId.Value);
        Assert.Equal(_id2, agg.CommandTypeRef.CommandDefinitionId);
        Assert.False(agg.IsRemoved);
    }

    [Fact]
    public void CommandRoutingRule_Register_InvalidRoute_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            CommandRoutingRuleAggregate.Register(
                new CommandRoutingRuleId(_id1),
                new CommandTypeRef(_id2),
                new DomainRoute("", "", ""),
                _ts));
    }

    [Fact]
    public void CommandRoutingRule_Remove_SetsIsRemoved()
    {
        var agg = CommandRoutingRuleAggregate.Register(
            new CommandRoutingRuleId(_id1),
            new CommandTypeRef(_id2),
            _handler,
            _ts);

        agg.Remove(_ts);

        Assert.True(agg.IsRemoved);
        Assert.Equal(2, agg.DomainEvents.Count);
    }

    [Fact]
    public void CommandRoutingRule_Remove_AlreadyRemoved_Throws()
    {
        var agg = CommandRoutingRuleAggregate.Register(
            new CommandRoutingRuleId(_id1),
            new CommandTypeRef(_id2),
            _handler,
            _ts);
        agg.Remove(_ts);

        Assert.Throws<DomainInvariantViolationException>(() => agg.Remove(_ts));
    }

    [Fact]
    public void CommandTypeRef_EmptyGuid_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() => new CommandTypeRef(Guid.Empty));
    }

    // -------------------------------------------------------------------------
    // PolicyContextRef
    // -------------------------------------------------------------------------

    [Fact]
    public void PolicyContextRef_WithPolicy_IsPresent()
    {
        var ref1 = new PolicyContextRef("policy-123", "1.0");
        Assert.True(ref1.IsPresent);
        Assert.Equal("policy-123", ref1.PolicyId);
    }

    [Fact]
    public void PolicyContextRef_NullPolicy_IsNotPresent()
    {
        var ref1 = new PolicyContextRef(null, null);
        Assert.False(ref1.IsPresent);
    }
}
