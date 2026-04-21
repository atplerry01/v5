using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Command.CommandEnvelope;

public sealed record CommandEnvelopeSealedEvent(
    [property: JsonPropertyName("AggregateId")] CommandEnvelopeId EnvelopeId,
    EnvelopeMetadata Metadata) : DomainEvent;
