namespace Whycespace.Shared.Contracts.Events.Content.Streaming.PlaybackConsumption.Replay;

public sealed record ReplayRequestedEventSchema(
    Guid AggregateId,
    Guid ArchiveId,
    Guid ViewerId,
    DateTimeOffset RequestedAt);

public sealed record ReplayStartedEventSchema(
    Guid AggregateId,
    long PositionMs,
    DateTimeOffset StartedAt);

public sealed record ReplayPausedEventSchema(
    Guid AggregateId,
    long PositionMs,
    DateTimeOffset PausedAt);

public sealed record ReplayResumedEventSchema(
    Guid AggregateId,
    DateTimeOffset ResumedAt);

public sealed record ReplayCompletedEventSchema(
    Guid AggregateId,
    long PositionMs,
    DateTimeOffset CompletedAt);

public sealed record ReplayAbandonedEventSchema(
    Guid AggregateId,
    DateTimeOffset AbandonedAt);
