using Whycespace.Shared.Contracts.Events.Platform.Envelope.MessageEnvelope;
using DomainEvents = Whycespace.Domain.PlatformSystem.Envelope.MessageEnvelope;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class PlatformMessageEnvelopeSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("MessageEnvelopeCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.MessageEnvelopeCreatedEvent), typeof(MessageEnvelopeCreatedEventSchema));
        sink.RegisterSchema("MessageEnvelopeDispatchedEvent", EventVersion.Default,
            typeof(DomainEvents.MessageEnvelopeDispatchedEvent), typeof(MessageEnvelopeDispatchedEventSchema));
        sink.RegisterSchema("MessageEnvelopeAcknowledgedEvent", EventVersion.Default,
            typeof(DomainEvents.MessageEnvelopeAcknowledgedEvent), typeof(MessageEnvelopeAcknowledgedEventSchema));
        sink.RegisterSchema("MessageEnvelopeRejectedEvent", EventVersion.Default,
            typeof(DomainEvents.MessageEnvelopeRejectedEvent), typeof(MessageEnvelopeRejectedEventSchema));

        sink.RegisterPayloadMapper("MessageEnvelopeCreatedEvent", e =>
        {
            var evt = (DomainEvents.MessageEnvelopeCreatedEvent)e;
            return new MessageEnvelopeCreatedEventSchema(evt.EnvelopeId.Value,
                evt.MessageKind.Value,
                evt.Metadata.CorrelationId, evt.Metadata.CausationId,
                evt.Header.SourceAddress.Classification, evt.Header.SourceAddress.Context, evt.Header.SourceAddress.Domain,
                evt.Metadata.IssuedAt.Value);
        });
        sink.RegisterPayloadMapper("MessageEnvelopeDispatchedEvent", e =>
        {
            var evt = (DomainEvents.MessageEnvelopeDispatchedEvent)e;
            return new MessageEnvelopeDispatchedEventSchema(evt.EnvelopeId.Value);
        });
        sink.RegisterPayloadMapper("MessageEnvelopeAcknowledgedEvent", e =>
        {
            var evt = (DomainEvents.MessageEnvelopeAcknowledgedEvent)e;
            return new MessageEnvelopeAcknowledgedEventSchema(evt.EnvelopeId.Value);
        });
        sink.RegisterPayloadMapper("MessageEnvelopeRejectedEvent", e =>
        {
            var evt = (DomainEvents.MessageEnvelopeRejectedEvent)e;
            return new MessageEnvelopeRejectedEventSchema(evt.EnvelopeId.Value, evt.RejectionReason);
        });
    }
}
