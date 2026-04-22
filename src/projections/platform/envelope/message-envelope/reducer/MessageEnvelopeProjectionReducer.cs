using Whycespace.Shared.Contracts.Events.Platform.Envelope.MessageEnvelope;
using Whycespace.Shared.Contracts.Platform.Envelope.MessageEnvelope;

namespace Whycespace.Projections.Platform.Envelope.MessageEnvelope.Reducer;

public static class MessageEnvelopeProjectionReducer
{
    public static MessageEnvelopeReadModel Apply(MessageEnvelopeReadModel state, MessageEnvelopeCreatedEventSchema e, DateTimeOffset at) =>
        state with
        {
            EnvelopeId = e.AggregateId,
            MessageKind = e.MessageKind,
            CorrelationId = e.CorrelationId,
            CausationId = e.CausationId,
            SourceClassification = e.SourceClassification,
            SourceContext = e.SourceContext,
            SourceDomain = e.SourceDomain,
            Status = "Pending",
            IssuedAt = e.IssuedAt,
            LastModifiedAt = at
        };

    public static MessageEnvelopeReadModel Apply(MessageEnvelopeReadModel state, MessageEnvelopeDispatchedEventSchema e, DateTimeOffset at) =>
        state with { Status = "Dispatched", LastModifiedAt = at };

    public static MessageEnvelopeReadModel Apply(MessageEnvelopeReadModel state, MessageEnvelopeAcknowledgedEventSchema e, DateTimeOffset at) =>
        state with { Status = "Acknowledged", LastModifiedAt = at };

    public static MessageEnvelopeReadModel Apply(MessageEnvelopeReadModel state, MessageEnvelopeRejectedEventSchema e, DateTimeOffset at) =>
        state with { Status = "Rejected", RejectionReason = e.RejectionReason, LastModifiedAt = at };
}
