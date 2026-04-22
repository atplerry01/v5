namespace Whycespace.Shared.Contracts.Events.Content.Streaming.LiveStreaming.IngestSession;

public sealed record IngestSessionAuthenticatedEventSchema(
    Guid AggregateId,
    Guid BroadcastId,
    string Endpoint,
    DateTimeOffset AuthenticatedAt);

public sealed record IngestStreamingStartedEventSchema(
    Guid AggregateId,
    DateTimeOffset StartedAt);

public sealed record IngestSessionStalledEventSchema(
    Guid AggregateId,
    DateTimeOffset StalledAt);

public sealed record IngestSessionResumedEventSchema(
    Guid AggregateId,
    DateTimeOffset ResumedAt);

public sealed record IngestSessionEndedEventSchema(
    Guid AggregateId,
    DateTimeOffset EndedAt);

public sealed record IngestSessionFailedEventSchema(
    Guid AggregateId,
    string FailureReason,
    DateTimeOffset FailedAt);
