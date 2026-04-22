using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.MessageEnvelope;

public sealed record MessageEnvelopeDispatchedEvent(
    [property: JsonPropertyName("AggregateId")] EnvelopeId EnvelopeId,
    Timestamp DispatchedAt) : DomainEvent;
