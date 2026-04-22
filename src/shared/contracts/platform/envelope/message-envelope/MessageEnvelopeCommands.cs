using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Platform.Envelope.MessageEnvelope;

public sealed record CreateMessageEnvelopeCommand(
    Guid EnvelopeId,
    Guid HeaderMessageId,
    string HeaderContentType,
    string HeaderMessageKindHint,
    string HeaderSourceClassification,
    string HeaderSourceContext,
    string HeaderSourceDomain,
    string? HeaderDestinationClassification,
    string? HeaderDestinationContext,
    string? HeaderDestinationDomain,
    string HeaderTraceId,
    string HeaderSpanId,
    string? HeaderParentSpanId,
    bool HeaderSamplingFlag,
    string PayloadTypeRef,
    string PayloadEncoding,
    string? PayloadSchemaRef,
    byte[] PayloadBytes,
    Guid MetadataCorrelationId,
    Guid MetadataCausationId,
    int MetadataMessageVersion,
    DateTimeOffset MetadataIssuedAt,
    Guid? MetadataTenantId,
    string MessageKind,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => EnvelopeId;
}

public sealed record DispatchMessageEnvelopeCommand(
    Guid EnvelopeId,
    DateTimeOffset DispatchedAt) : IHasAggregateId
{
    public Guid AggregateId => EnvelopeId;
}

public sealed record AcknowledgeMessageEnvelopeCommand(
    Guid EnvelopeId,
    DateTimeOffset AcknowledgedAt) : IHasAggregateId
{
    public Guid AggregateId => EnvelopeId;
}

public sealed record RejectMessageEnvelopeCommand(
    Guid EnvelopeId,
    string RejectionReason,
    DateTimeOffset RejectedAt) : IHasAggregateId
{
    public Guid AggregateId => EnvelopeId;
}
