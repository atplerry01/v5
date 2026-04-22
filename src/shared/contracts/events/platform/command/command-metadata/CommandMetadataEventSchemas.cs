namespace Whycespace.Shared.Contracts.Events.Platform.Command.CommandMetadata;

public sealed record CommandMetadataAttachedEventSchema(
    Guid AggregateId,
    Guid EnvelopeRef,
    string ActorId,
    string TraceId,
    string SpanId,
    string? PolicyId,
    string? PolicyVersion,
    int TrustScore,
    DateTimeOffset IssuedAt);
