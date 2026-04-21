namespace Whycespace.Shared.Contracts.Events.Content.Streaming.LiveStreaming.Archive;

public sealed record ArchiveStartedEventSchema(
    Guid AggregateId,
    Guid StreamId,
    Guid? SessionId,
    DateTimeOffset StartedAt);

public sealed record ArchiveCompletedEventSchema(
    Guid AggregateId,
    Guid OutputId,
    DateTimeOffset CompletedAt);

public sealed record ArchiveFailedEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset FailedAt);

public sealed record ArchiveFinalizedEventSchema(
    Guid AggregateId,
    DateTimeOffset FinalizedAt);

public sealed record ArchiveArchivedEventSchema(
    Guid AggregateId,
    DateTimeOffset ArchivedAt);
