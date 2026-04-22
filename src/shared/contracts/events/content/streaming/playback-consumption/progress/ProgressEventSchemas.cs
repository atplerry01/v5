namespace Whycespace.Shared.Contracts.Events.Content.Streaming.PlaybackConsumption.Progress;

public sealed record ProgressTrackedEventSchema(
    Guid AggregateId,
    Guid SessionId,
    long PositionMs,
    DateTimeOffset TrackedAt);

public sealed record PlaybackPositionUpdatedEventSchema(
    Guid AggregateId,
    long PositionMs,
    DateTimeOffset UpdatedAt);

public sealed record PlaybackPausedEventSchema(
    Guid AggregateId,
    long PositionMs,
    DateTimeOffset PausedAt);

public sealed record PlaybackResumedEventSchema(
    Guid AggregateId,
    DateTimeOffset ResumedAt);

public sealed record ProgressTerminatedEventSchema(
    Guid AggregateId,
    DateTimeOffset TerminatedAt);
