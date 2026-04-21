using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Event.EventEnvelope;

public sealed record EventEnvelopeSealedEvent(
    [property: JsonPropertyName("AggregateId")] EventEnvelopeId EnvelopeId,
    EventEnvelopeMetadata Metadata) : DomainEvent;
