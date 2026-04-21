namespace Whycespace.Shared.Contracts.Events.Content.Media.CoreObject.Transcript;

public sealed record TranscriptCreatedEventSchema(
    Guid AggregateId,
    Guid AssetRef,
    Guid? SourceFileRef,
    string Format,
    string Language,
    DateTimeOffset CreatedAt);

public sealed record TranscriptUpdatedEventSchema(
    Guid AggregateId,
    Guid OutputRef,
    DateTimeOffset UpdatedAt);

public sealed record TranscriptFinalizedEventSchema(
    Guid AggregateId,
    DateTimeOffset FinalizedAt);

public sealed record TranscriptArchivedEventSchema(
    Guid AggregateId,
    DateTimeOffset ArchivedAt);
