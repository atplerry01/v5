using Whycespace.Domain.PlatformSystem.Schema.Contract;
using Whycespace.Domain.PlatformSystem.Schema.SchemaDefinition;
using Whycespace.Domain.PlatformSystem.Schema.Serialization;
using Whycespace.Domain.PlatformSystem.Schema.Versioning;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Tests.Unit.PlatformSystem.Schema;

/// <summary>
/// E12 — Domain tests for the schema context: contract, schema-definition, serialization, versioning.
/// Covers topics 6 (aggregate design), 26 (test certification) of platform-system.md.
/// </summary>
public sealed class PlatformSystemSchemaTests
{
    private static readonly Guid _id1 = new("eeeeeeee-0000-0000-0000-000000000001");
    private static readonly Guid _id2 = new("eeeeeeee-0000-0000-0000-000000000002");
    private static readonly Guid _id3 = new("eeeeeeee-0000-0000-0000-000000000003");
    private static readonly Timestamp _ts = new(new DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero));
    private static readonly DomainRoute _publisher = new("economic-system", "capital", "vault");
    private static readonly DomainRoute _subscriber = new("platform-system", "command", "command-definition");

    // -------------------------------------------------------------------------
    // ContractAggregate
    // -------------------------------------------------------------------------

    [Fact]
    public void Contract_Register_ValidInputs_SetsState()
    {
        var agg = ContractAggregate.Register(
            new ContractId(_id1),
            "VaultFundedContract",
            _publisher,
            _id2,
            1,
            _ts);

        Assert.Equal(_id1, agg.ContractId.Value);
        Assert.Equal("VaultFundedContract", agg.ContractName);
        Assert.Equal(_publisher, agg.PublisherRoute);
        Assert.Equal(_id2, agg.SchemaRef);
        Assert.Equal(ContractStatus.Active, agg.Status);
        Assert.Empty(agg.SubscriberConstraints);
    }

    [Fact]
    public void Contract_Register_RaisesRegisteredEvent()
    {
        var agg = ContractAggregate.Register(
            new ContractId(_id1),
            "VaultFundedContract",
            _publisher,
            _id2,
            1,
            _ts);

        Assert.Single(agg.DomainEvents);
        Assert.IsType<ContractRegisteredEvent>(agg.DomainEvents[0]);
    }

    [Fact]
    public void Contract_Register_EmptyContractName_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            ContractAggregate.Register(
                new ContractId(_id1),
                "",
                _publisher,
                _id2,
                1,
                _ts));
    }

    [Fact]
    public void Contract_Register_EmptySchemaRef_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            ContractAggregate.Register(
                new ContractId(_id1),
                "SomeContract",
                _publisher,
                Guid.Empty,
                1,
                _ts));
    }

    [Fact]
    public void Contract_AddSubscriber_AppendsConstraint()
    {
        var agg = ContractAggregate.Register(
            new ContractId(_id1),
            "VaultContract",
            _publisher,
            _id2,
            1,
            _ts);

        var constraint = new SubscriberConstraint(_subscriber, 1, ContractCompatibilityMode.Backward);
        agg.AddSubscriber(constraint, _ts);

        Assert.Single(agg.SubscriberConstraints);
        Assert.Equal(_subscriber, agg.SubscriberConstraints[0].SubscriberRoute);
    }

    [Fact]
    public void Contract_AddSubscriber_AfterDeprecation_Throws()
    {
        var agg = ContractAggregate.Register(
            new ContractId(_id1),
            "VaultContract",
            _publisher,
            _id2,
            1,
            _ts);
        agg.Deprecate(_ts);

        var constraint = new SubscriberConstraint(_subscriber, 1, ContractCompatibilityMode.Full);
        Assert.Throws<DomainInvariantViolationException>(() => agg.AddSubscriber(constraint, _ts));
    }

    [Fact]
    public void Contract_Deprecate_ChangesStatus()
    {
        var agg = ContractAggregate.Register(
            new ContractId(_id1),
            "VaultContract",
            _publisher,
            _id2,
            1,
            _ts);

        agg.Deprecate(_ts);

        Assert.Equal(ContractStatus.Deprecated, agg.Status);
    }

    [Fact]
    public void SubscriberConstraint_InvalidMinSchemaVersion_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            new SubscriberConstraint(_subscriber, 0, ContractCompatibilityMode.Backward));
    }

    // -------------------------------------------------------------------------
    // SchemaDefinitionAggregate
    // -------------------------------------------------------------------------

    private static FieldDescriptor MakeField(string name) =>
        new(name, FieldType.String, true, null, null);

    [Fact]
    public void SchemaDefinition_Draft_ValidInputs_SetsState()
    {
        var fields = new List<FieldDescriptor> { MakeField("aggregateId"), MakeField("occurredAt") };
        var agg = SchemaDefinitionAggregate.Draft(
            new SchemaDefinitionId(_id1),
            new SchemaName("VaultFundedEventSchema"),
            1,
            fields,
            SchemaCompatibilityMode.Backward,
            _ts);

        Assert.Equal(_id1, agg.SchemaDefinitionId.Value);
        Assert.Equal("VaultFundedEventSchema", agg.SchemaName.Value);
        Assert.Equal(2, agg.Fields.Count);
        Assert.Equal(SchemaStatus.Draft, agg.Status);
    }

    [Fact]
    public void SchemaDefinition_Draft_EmptyFields_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            SchemaDefinitionAggregate.Draft(
                new SchemaDefinitionId(_id1),
                new SchemaName("SomeSchema"),
                1,
                [],
                SchemaCompatibilityMode.None,
                _ts));
    }

    [Fact]
    public void SchemaDefinition_Publish_TransitionsToPublished()
    {
        var fields = new List<FieldDescriptor> { MakeField("eventId") };
        var agg = SchemaDefinitionAggregate.Draft(
            new SchemaDefinitionId(_id1),
            new SchemaName("SomeSchema"),
            1,
            fields,
            SchemaCompatibilityMode.Backward,
            _ts);

        agg.Publish(_ts);

        Assert.Equal(SchemaStatus.Published, agg.Status);
        Assert.Equal(2, agg.DomainEvents.Count);
    }

    [Fact]
    public void SchemaDefinition_Publish_AlreadyPublished_Throws()
    {
        var fields = new List<FieldDescriptor> { MakeField("eventId") };
        var agg = SchemaDefinitionAggregate.Draft(
            new SchemaDefinitionId(_id1),
            new SchemaName("SomeSchema"),
            1,
            fields,
            SchemaCompatibilityMode.Full,
            _ts);
        agg.Publish(_ts);

        Assert.Throws<DomainInvariantViolationException>(() => agg.Publish(_ts));
    }

    [Fact]
    public void SchemaDefinition_Deprecate_ChangesStatus()
    {
        var fields = new List<FieldDescriptor> { MakeField("eventId") };
        var agg = SchemaDefinitionAggregate.Draft(
            new SchemaDefinitionId(_id1),
            new SchemaName("SomeSchema"),
            1,
            fields,
            SchemaCompatibilityMode.None,
            _ts);

        agg.Deprecate(_ts);

        Assert.Equal(SchemaStatus.Deprecated, agg.Status);
    }

    [Fact]
    public void FieldDescriptor_EmptyFieldName_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            new FieldDescriptor("", FieldType.String, true, null, null));
    }

    [Fact]
    public void FieldType_StaticValues_AreDistinct()
    {
        Assert.NotEqual(FieldType.String, FieldType.Int);
        Assert.NotEqual(FieldType.Int, FieldType.Long);
        Assert.NotEqual(FieldType.Bool, FieldType.Bytes);
    }

    // -------------------------------------------------------------------------
    // SerializationFormatAggregate
    // -------------------------------------------------------------------------

    [Fact]
    public void SerializationFormat_Register_JsonLossless_SetsState()
    {
        var agg = SerializationFormatAggregate.Register(
            new SerializationFormatId(_id1),
            "json-v1",
            SerializationEncoding.Json,
            null,
            [],
            RoundTripGuarantee.Lossless,
            1,
            _ts);

        Assert.Equal("json-v1", agg.FormatName);
        Assert.Equal(SerializationEncoding.Json, agg.Encoding);
        Assert.Equal(RoundTripGuarantee.Lossless, agg.RoundTripGuarantee);
        Assert.Equal(SerializationFormatStatus.Active, agg.Status);
    }

    [Fact]
    public void SerializationFormat_Register_EmptyFormatName_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            SerializationFormatAggregate.Register(
                new SerializationFormatId(_id1),
                "",
                SerializationEncoding.Json,
                null,
                [],
                RoundTripGuarantee.Lossless,
                1,
                _ts));
    }

    [Fact]
    public void SerializationFormat_Register_AvroWithoutSchemaRef_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            SerializationFormatAggregate.Register(
                new SerializationFormatId(_id1),
                "avro-format",
                SerializationEncoding.Avro,
                null,
                [],
                RoundTripGuarantee.Lossless,
                1,
                _ts));
    }

    [Fact]
    public void SerializationFormat_Register_LossyWithoutDocumentation_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            SerializationFormatAggregate.Register(
                new SerializationFormatId(_id1),
                "lossy-format",
                SerializationEncoding.Json,
                null,
                [],
                RoundTripGuarantee.LossyWithDocumentedFields,
                1,
                _ts));
    }

    [Fact]
    public void SerializationFormat_Register_LossyWithDocumentation_Succeeds()
    {
        var options = new List<SerializationOption> { new SerializationOption("lossyField", "timestamp-precision") };
        var agg = SerializationFormatAggregate.Register(
            new SerializationFormatId(_id1),
            "lossy-json",
            SerializationEncoding.Json,
            null,
            options,
            RoundTripGuarantee.LossyWithDocumentedFields,
            1,
            _ts);

        Assert.Equal(RoundTripGuarantee.LossyWithDocumentedFields, agg.RoundTripGuarantee);
        Assert.Single(agg.Options);
    }

    [Fact]
    public void SerializationFormat_Deprecate_ChangesStatus()
    {
        var agg = SerializationFormatAggregate.Register(
            new SerializationFormatId(_id1),
            "json-v1",
            SerializationEncoding.Json,
            null,
            [],
            RoundTripGuarantee.Lossless,
            1,
            _ts);

        agg.Deprecate(_ts);

        Assert.Equal(SerializationFormatStatus.Deprecated, agg.Status);
    }

    // -------------------------------------------------------------------------
    // VersioningRuleAggregate
    // -------------------------------------------------------------------------

    private static SchemaChange MakeChange() =>
        new(SchemaChangeType.FieldAdded, "newField", ChangeImpact.Safe);

    [Fact]
    public void VersioningRule_Register_ValidInputs_SetsState()
    {
        var changes = new List<SchemaChange> { MakeChange() };
        var agg = VersioningRuleAggregate.Register(
            new VersioningRuleId(_id1),
            _id2,
            1,
            2,
            EvolutionClass.NonBreaking,
            changes,
            _ts);

        Assert.Equal(_id1, agg.VersioningRuleId.Value);
        Assert.Equal(_id2, agg.SchemaRef);
        Assert.Equal(1, agg.FromVersion);
        Assert.Equal(2, agg.ToVersion);
        Assert.Equal(EvolutionClass.NonBreaking, agg.EvolutionClass);
        Assert.Null(agg.Verdict);
    }

    [Fact]
    public void VersioningRule_Register_InvalidVersionOrder_Throws()
    {
        var changes = new List<SchemaChange> { MakeChange() };
        Assert.Throws<DomainInvariantViolationException>(() =>
            VersioningRuleAggregate.Register(
                new VersioningRuleId(_id1),
                _id2,
                5,
                3,
                EvolutionClass.Breaking,
                changes,
                _ts));
    }

    [Fact]
    public void VersioningRule_Register_EmptyChangeSummary_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            VersioningRuleAggregate.Register(
                new VersioningRuleId(_id1),
                _id2,
                1,
                2,
                EvolutionClass.NonBreaking,
                [],
                _ts));
    }

    [Fact]
    public void VersioningRule_Register_EmptySchemaRef_Throws()
    {
        var changes = new List<SchemaChange> { MakeChange() };
        Assert.Throws<DomainInvariantViolationException>(() =>
            VersioningRuleAggregate.Register(
                new VersioningRuleId(_id1),
                Guid.Empty,
                1,
                2,
                EvolutionClass.NonBreaking,
                changes,
                _ts));
    }

    [Fact]
    public void VersioningRule_IssueVerdict_SetsVerdict()
    {
        var changes = new List<SchemaChange> { MakeChange() };
        var agg = VersioningRuleAggregate.Register(
            new VersioningRuleId(_id1),
            _id2,
            1,
            2,
            EvolutionClass.NonBreaking,
            changes,
            _ts);

        agg.IssueVerdict(CompatibilityVerdict.Compatible, _ts);

        Assert.Equal(CompatibilityVerdict.Compatible, agg.Verdict);
        Assert.Equal(2, agg.DomainEvents.Count);
    }

    [Fact]
    public void VersioningRule_IssueVerdict_AlreadyIssued_Throws()
    {
        var changes = new List<SchemaChange> { MakeChange() };
        var agg = VersioningRuleAggregate.Register(
            new VersioningRuleId(_id1),
            _id2,
            1,
            2,
            EvolutionClass.NonBreaking,
            changes,
            _ts);
        agg.IssueVerdict(CompatibilityVerdict.Compatible, _ts);

        Assert.Throws<DomainInvariantViolationException>(() => agg.IssueVerdict(CompatibilityVerdict.Incompatible, _ts));
    }

    [Fact]
    public void EvolutionClass_StaticValues_AreDistinct()
    {
        Assert.NotEqual(EvolutionClass.NonBreaking, EvolutionClass.Breaking);
        Assert.NotEqual(EvolutionClass.Breaking, EvolutionClass.Incompatible);
    }
}
