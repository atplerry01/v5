namespace Whycespace.Domain.CoreSystem.Event.EventEnvelope;

public sealed record EventEnvelopeSealedEvent(
    EventEnvelopeId EnvelopeId,
    EventEnvelopeMetadata Metadata);
