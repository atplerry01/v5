namespace Whycespace.Shared.Contracts.Events.Content.Streaming.PlaybackConsumption.Session;

public sealed record SessionOpenedEventSchema(
    Guid AggregateId,
    Guid StreamId,
    DateTimeOffset OpenedAt,
    DateTimeOffset ExpiresAt);

public sealed record SessionActivatedEventSchema(
    Guid AggregateId,
    DateTimeOffset ActivatedAt);

public sealed record SessionSuspendedEventSchema(
    Guid AggregateId,
    DateTimeOffset SuspendedAt);

public sealed record SessionResumedEventSchema(
    Guid AggregateId,
    DateTimeOffset ResumedAt);

public sealed record SessionClosedEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset ClosedAt);

public sealed record SessionFailedEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset FailedAt);

public sealed record SessionExpiredEventSchema(
    Guid AggregateId,
    DateTimeOffset ExpiredAt);
