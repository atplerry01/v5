namespace Whycespace.Shared.Contracts.Events.Platform.Envelope.MessageEnvelope;

public sealed record MessageEnvelopeCreatedEventSchema(
    Guid AggregateId,
    string MessageKind,
    Guid CorrelationId,
    Guid CausationId,
    string SourceClassification,
    string SourceContext,
    string SourceDomain,
    DateTimeOffset IssuedAt);

public sealed record MessageEnvelopeDispatchedEventSchema(Guid AggregateId);

public sealed record MessageEnvelopeAcknowledgedEventSchema(Guid AggregateId);

public sealed record MessageEnvelopeRejectedEventSchema(Guid AggregateId, string RejectionReason);
