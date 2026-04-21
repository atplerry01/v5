namespace Whycespace.Shared.Contracts.Events.Content.Streaming.StreamCore.Stream;

public sealed record StreamCreatedEventSchema(
    Guid AggregateId,
    string Mode,
    string Type,
    DateTimeOffset CreatedAt);

public sealed record StreamActivatedEventSchema(
    Guid AggregateId,
    DateTimeOffset ActivatedAt);

public sealed record StreamPausedEventSchema(
    Guid AggregateId,
    DateTimeOffset PausedAt);

public sealed record StreamResumedEventSchema(
    Guid AggregateId,
    DateTimeOffset ResumedAt);

public sealed record StreamEndedEventSchema(
    Guid AggregateId,
    DateTimeOffset EndedAt);

public sealed record StreamArchivedEventSchema(
    Guid AggregateId,
    DateTimeOffset ArchivedAt);
