using System.Text.Json.Serialization;
using Whycespace.Domain.PlatformSystem.Envelope.Header;
using Whycespace.Domain.PlatformSystem.Envelope.Metadata;
using Whycespace.Domain.PlatformSystem.Envelope.Payload;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.MessageEnvelope;

public sealed record MessageEnvelopeCreatedEvent(
    [property: JsonPropertyName("AggregateId")] EnvelopeId EnvelopeId,
    HeaderValueObject Header,
    PayloadValueObject Payload,
    MessageMetadataValueObject Metadata,
    MessageKind MessageKind,
    Timestamp CreatedAt) : DomainEvent;
