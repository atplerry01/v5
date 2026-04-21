namespace Whycespace.Shared.Contracts.Events.Content.Streaming.LiveStreaming.Broadcast;

public sealed record BroadcastCreatedEventSchema(
    Guid AggregateId,
    Guid StreamId,
    DateTimeOffset CreatedAt);

public sealed record BroadcastScheduledEventSchema(
    Guid AggregateId,
    DateTimeOffset ScheduledStart,
    DateTimeOffset ScheduledEnd,
    DateTimeOffset ScheduledAt);

public sealed record BroadcastStartedEventSchema(
    Guid AggregateId,
    DateTimeOffset StartedAt);

public sealed record BroadcastPausedEventSchema(
    Guid AggregateId,
    DateTimeOffset PausedAt);

public sealed record BroadcastResumedEventSchema(
    Guid AggregateId,
    DateTimeOffset ResumedAt);

public sealed record BroadcastEndedEventSchema(
    Guid AggregateId,
    DateTimeOffset EndedAt);

public sealed record BroadcastCancelledEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset CancelledAt);
