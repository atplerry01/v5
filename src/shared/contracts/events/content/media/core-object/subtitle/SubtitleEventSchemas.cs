namespace Whycespace.Shared.Contracts.Events.Content.Media.CoreObject.Subtitle;

public sealed record SubtitleCreatedEventSchema(
    Guid AggregateId,
    Guid AssetRef,
    Guid? SourceFileRef,
    string Format,
    string Language,
    long? WindowStartMs,
    long? WindowEndMs,
    DateTimeOffset CreatedAt);

public sealed record SubtitleUpdatedEventSchema(
    Guid AggregateId,
    Guid OutputRef,
    DateTimeOffset UpdatedAt);

public sealed record SubtitleFinalizedEventSchema(
    Guid AggregateId,
    DateTimeOffset FinalizedAt);

public sealed record SubtitleArchivedEventSchema(
    Guid AggregateId,
    DateTimeOffset ArchivedAt);
