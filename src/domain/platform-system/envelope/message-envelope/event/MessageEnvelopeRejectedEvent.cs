using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.MessageEnvelope;

public sealed record MessageEnvelopeRejectedEvent(
    [property: JsonPropertyName("AggregateId")] EnvelopeId EnvelopeId,
    string RejectionReason,
    Timestamp RejectedAt) : DomainEvent;
