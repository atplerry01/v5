using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.MessageEnvelope;

public sealed record MessageEnvelopeAcknowledgedEvent(
    [property: JsonPropertyName("AggregateId")] EnvelopeId EnvelopeId,
    Timestamp AcknowledgedAt) : DomainEvent;
