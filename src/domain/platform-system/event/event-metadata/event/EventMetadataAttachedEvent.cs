using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventMetadata;

public sealed record EventMetadataAttachedEvent(
    [property: JsonPropertyName("AggregateId")] EventMetadataId EventMetadataId,
    EventEnvelopeRef EnvelopeRef,
    ExecutionHash ExecutionHash,
    PolicyDecisionHash PolicyDecisionHash,
    EventActorId ActorId,
    EventTraceId TraceId,
    EventSpanId SpanId,
    Timestamp IssuedAt) : DomainEvent;
