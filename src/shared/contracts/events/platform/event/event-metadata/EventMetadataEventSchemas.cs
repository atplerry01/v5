namespace Whycespace.Shared.Contracts.Events.Platform.Event.EventMetadata;

public sealed record EventMetadataAttachedEventSchema(
    Guid AggregateId,
    Guid EnvelopeRef,
    string ExecutionHash,
    string PolicyDecisionHash,
    string ActorId,
    string TraceId,
    string SpanId,
    DateTimeOffset IssuedAt);
