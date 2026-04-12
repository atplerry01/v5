namespace Whycespace.Domain.CoreSystem.Command.CommandEnvelope;

public sealed record CommandEnvelopeSealedEvent(
    CommandEnvelopeId EnvelopeId,
    EnvelopeMetadata Metadata);
