using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandMetadata;

public sealed record CommandMetadataAttachedEvent(
    [property: JsonPropertyName("AggregateId")] CommandMetadataId CommandMetadataId,
    Guid EnvelopeRef,
    MetadataActorId ActorId,
    MetadataTraceId TraceId,
    MetadataSpanId SpanId,
    PolicyContextRef PolicyContextRef,
    TrustScore TrustScore,
    Timestamp IssuedAt) : DomainEvent;
