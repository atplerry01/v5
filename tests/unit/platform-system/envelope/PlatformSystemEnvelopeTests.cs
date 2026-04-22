using Whycespace.Domain.PlatformSystem.Envelope.Header;
using Whycespace.Domain.PlatformSystem.Envelope.MessageEnvelope;
using Whycespace.Domain.PlatformSystem.Envelope.Metadata;
using Whycespace.Domain.PlatformSystem.Envelope.Payload;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Tests.Unit.PlatformSystem.Envelope;

/// <summary>
/// E12 — Domain tests for the envelope context: header, message-envelope, metadata, payload.
/// Covers topics 6 (aggregate design), 26 (test certification) of platform-system.md.
/// </summary>
public sealed class PlatformSystemEnvelopeTests
{
    private static readonly Guid _id1 = new("bbbbbbbb-0000-0000-0000-000000000001");
    private static readonly Guid _id2 = new("bbbbbbbb-0000-0000-0000-000000000002");
    private static readonly Timestamp _ts = new(new DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero));
    private static readonly DomainRoute _source = new("platform-system", "envelope", "message-envelope");
    private static readonly DomainRoute _dest = new("platform-system", "command", "command-definition");

    private static HeaderValueObject MakeHeader() => new(
        _id1,
        "application/json",
        "Command",
        _source,
        _dest,
        "trace-aaa",
        "span-001",
        null,
        false);

    private static PayloadValueObject MakePayload() => new(
        "platform-system/command/DefineVault",
        PayloadEncoding.Json,
        null,
        new byte[] { 1, 2, 3 });

    private static MessageMetadataValueObject MakeMetadata() => new(
        _id1,
        _id2,
        1,
        _ts,
        null);

    // -------------------------------------------------------------------------
    // HeaderSchemaAggregate
    // -------------------------------------------------------------------------

    [Fact]
    public void HeaderSchema_Register_WithMandatoryFields_SetsState()
    {
        var required = new List<string> { "MessageId", "ContentType", "SourceAddress" };
        var agg = HeaderSchemaAggregate.Register(
            new HeaderSchemaId(_id1),
            new HeaderKind("Command"),
            1,
            required,
            _ts);

        Assert.Equal(_id1, agg.HeaderSchemaId.Value);
        Assert.Equal("Command", agg.HeaderKind.Value);
        Assert.Equal(1, agg.SchemaVersion);
        Assert.Equal(HeaderSchemaStatus.Active, agg.Status);
    }

    [Fact]
    public void HeaderSchema_Register_MissingMandatoryField_Throws()
    {
        var incomplete = new List<string> { "MessageId", "ContentType" }; // missing SourceAddress
        Assert.Throws<DomainInvariantViolationException>(() =>
            HeaderSchemaAggregate.Register(
                new HeaderSchemaId(_id1),
                new HeaderKind("Command"),
                1,
                incomplete,
                _ts));
    }

    [Fact]
    public void HeaderSchema_Register_RaisesRegisteredEvent()
    {
        var required = new List<string> { "MessageId", "ContentType", "SourceAddress" };
        var agg = HeaderSchemaAggregate.Register(
            new HeaderSchemaId(_id1),
            new HeaderKind("Event"),
            2,
            required,
            _ts);

        Assert.Single(agg.DomainEvents);
        Assert.IsType<HeaderSchemaRegisteredEvent>(agg.DomainEvents[0]);
    }

    [Fact]
    public void HeaderSchema_Deprecate_ChangesStatus()
    {
        var required = new List<string> { "MessageId", "ContentType", "SourceAddress" };
        var agg = HeaderSchemaAggregate.Register(
            new HeaderSchemaId(_id1),
            new HeaderKind("Event"),
            1,
            required,
            _ts);

        agg.Deprecate(_ts);

        Assert.Equal(HeaderSchemaStatus.Deprecated, agg.Status);
    }

    [Fact]
    public void HeaderSchema_Deprecate_AlreadyDeprecated_Throws()
    {
        var required = new List<string> { "MessageId", "ContentType", "SourceAddress" };
        var agg = HeaderSchemaAggregate.Register(
            new HeaderSchemaId(_id1),
            new HeaderKind("Event"),
            1,
            required,
            _ts);
        agg.Deprecate(_ts);

        Assert.Throws<DomainInvariantViolationException>(() => agg.Deprecate(_ts));
    }

    // -------------------------------------------------------------------------
    // MessageEnvelopeAggregate — lifecycle
    // -------------------------------------------------------------------------

    [Fact]
    public void MessageEnvelope_Create_ValidInputs_SetsState()
    {
        var agg = MessageEnvelopeAggregate.Create(
            new EnvelopeId(_id1),
            MakeHeader(),
            MakePayload(),
            MakeMetadata(),
            MessageKind.Command,
            _ts);

        Assert.Equal(_id1, agg.EnvelopeId.Value);
        Assert.Equal(MessageKind.Command, agg.MessageKind);
        Assert.Equal(EnvelopeStatus.Created, agg.Status);
    }

    [Fact]
    public void MessageEnvelope_Create_RaisesCreatedEvent()
    {
        var agg = MessageEnvelopeAggregate.Create(
            new EnvelopeId(_id1),
            MakeHeader(),
            MakePayload(),
            MakeMetadata(),
            MessageKind.Event,
            _ts);

        Assert.Single(agg.DomainEvents);
        Assert.IsType<MessageEnvelopeCreatedEvent>(agg.DomainEvents[0]);
    }

    [Fact]
    public void MessageEnvelope_Dispatch_TransitionsToDispatched()
    {
        var agg = MessageEnvelopeAggregate.Create(
            new EnvelopeId(_id1),
            MakeHeader(),
            MakePayload(),
            MakeMetadata(),
            MessageKind.Command,
            _ts);

        agg.Dispatch(_ts);

        Assert.Equal(EnvelopeStatus.Dispatched, agg.Status);
        Assert.Equal(2, agg.DomainEvents.Count);
        Assert.IsType<MessageEnvelopeDispatchedEvent>(agg.DomainEvents[1]);
    }

    [Fact]
    public void MessageEnvelope_Acknowledge_AfterDispatch_TransitionsToAcknowledged()
    {
        var agg = MessageEnvelopeAggregate.Create(
            new EnvelopeId(_id1),
            MakeHeader(),
            MakePayload(),
            MakeMetadata(),
            MessageKind.Command,
            _ts);
        agg.Dispatch(_ts);

        agg.Acknowledge(_ts);

        Assert.Equal(EnvelopeStatus.Acknowledged, agg.Status);
        Assert.True(agg.Status.IsTerminal);
    }

    [Fact]
    public void MessageEnvelope_Reject_AfterDispatch_TransitionsToRejected()
    {
        var agg = MessageEnvelopeAggregate.Create(
            new EnvelopeId(_id1),
            MakeHeader(),
            MakePayload(),
            MakeMetadata(),
            MessageKind.Command,
            _ts);
        agg.Dispatch(_ts);

        agg.Reject("invalid-payload", _ts);

        Assert.Equal(EnvelopeStatus.Rejected, agg.Status);
        Assert.True(agg.Status.IsTerminal);
    }

    [Fact]
    public void MessageEnvelope_Dispatch_AfterTerminal_Throws()
    {
        var agg = MessageEnvelopeAggregate.Create(
            new EnvelopeId(_id1),
            MakeHeader(),
            MakePayload(),
            MakeMetadata(),
            MessageKind.Command,
            _ts);
        agg.Dispatch(_ts);
        agg.Acknowledge(_ts);

        Assert.Throws<DomainInvariantViolationException>(() => agg.Dispatch(_ts));
    }

    [Fact]
    public void MessageEnvelope_Acknowledge_BeforeDispatch_Throws()
    {
        var agg = MessageEnvelopeAggregate.Create(
            new EnvelopeId(_id1),
            MakeHeader(),
            MakePayload(),
            MakeMetadata(),
            MessageKind.Command,
            _ts);

        Assert.Throws<DomainInvariantViolationException>(() => agg.Acknowledge(_ts));
    }

    // -------------------------------------------------------------------------
    // MessageMetadataSchemaAggregate
    // -------------------------------------------------------------------------

    [Fact]
    public void MessageMetadataSchema_Register_WithMandatoryFields_SetsState()
    {
        var required = new List<string> { "CorrelationId", "CausationId", "IssuedAt", "MessageVersion" };
        var agg = MessageMetadataSchemaAggregate.Register(
            new MetadataSchemaId(_id1),
            1,
            required,
            new List<string> { "TenantId" },
            _ts);

        Assert.Equal(_id1, agg.MetadataSchemaId.Value);
        Assert.Equal(1, agg.SchemaVersion);
        Assert.Equal(4, agg.RequiredFields.Count);
    }

    [Fact]
    public void MessageMetadataSchema_Register_MissingMandatoryField_Throws()
    {
        var incomplete = new List<string> { "CorrelationId", "CausationId", "IssuedAt" }; // missing MessageVersion
        Assert.Throws<DomainInvariantViolationException>(() =>
            MessageMetadataSchemaAggregate.Register(
                new MetadataSchemaId(_id1),
                1,
                incomplete,
                [],
                _ts));
    }

    // -------------------------------------------------------------------------
    // PayloadSchemaAggregate
    // -------------------------------------------------------------------------

    [Fact]
    public void PayloadSchema_Register_JsonEncoding_NoSchemaRefRequired()
    {
        var agg = PayloadSchemaAggregate.Register(
            new PayloadSchemaId(_id1),
            "platform-system/command/DefineVault",
            PayloadEncoding.Json,
            null,
            1,
            null,
            _ts);

        Assert.Equal("platform-system/command/DefineVault", agg.TypeRef);
        Assert.Equal(PayloadEncoding.Json, agg.Encoding);
        Assert.Equal(PayloadSchemaStatus.Active, agg.Status);
    }

    [Fact]
    public void PayloadSchema_Register_AvroEncoding_WithoutSchemaRef_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            PayloadSchemaAggregate.Register(
                new PayloadSchemaId(_id1),
                "some-type",
                PayloadEncoding.Avro,
                null,
                1,
                null,
                _ts));
    }

    [Fact]
    public void PayloadSchema_Register_EmptyTypeRef_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            PayloadSchemaAggregate.Register(
                new PayloadSchemaId(_id1),
                "",
                PayloadEncoding.Json,
                null,
                1,
                null,
                _ts));
    }

    [Fact]
    public void PayloadSchema_Deprecate_ChangesStatus()
    {
        var agg = PayloadSchemaAggregate.Register(
            new PayloadSchemaId(_id1),
            "platform-system/command/DefineVault",
            PayloadEncoding.Json,
            null,
            1,
            null,
            _ts);

        agg.Deprecate(_ts);

        Assert.Equal(PayloadSchemaStatus.Deprecated, agg.Status);
    }

    // -------------------------------------------------------------------------
    // PayloadValueObject — VO validation
    // -------------------------------------------------------------------------

    [Fact]
    public void PayloadValueObject_JsonEncoding_NoSchemaRef_Accepted()
    {
        var payload = new PayloadValueObject("type-ref", PayloadEncoding.Json, null, new byte[] { 1 });
        Assert.Equal(PayloadEncoding.Json, payload.Encoding);
    }

    [Fact]
    public void PayloadValueObject_EmptyBytes_Throws()
    {
        Assert.Throws<DomainInvariantViolationException>(() =>
            new PayloadValueObject("type-ref", PayloadEncoding.Json, null, ReadOnlyMemory<byte>.Empty));
    }

    [Fact]
    public void MessageKind_StaticValues_AreDistinct()
    {
        Assert.NotEqual(MessageKind.Command, MessageKind.Event);
        Assert.NotEqual(MessageKind.Event, MessageKind.Notification);
        Assert.NotEqual(MessageKind.Notification, MessageKind.Query);
    }
}
