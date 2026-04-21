namespace Whycespace.Shared.Contracts.Events.Content.Streaming.StreamCore.Manifest;

public sealed record ManifestCreatedEventSchema(
    Guid AggregateId,
    Guid SourceId,
    string SourceKind,
    int InitialVersion,
    DateTimeOffset CreatedAt);

public sealed record ManifestUpdatedEventSchema(
    Guid AggregateId,
    int PreviousVersion,
    int NewVersion,
    DateTimeOffset UpdatedAt);

public sealed record ManifestPublishedEventSchema(
    Guid AggregateId,
    int Version,
    DateTimeOffset PublishedAt);

public sealed record ManifestRetiredEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset RetiredAt);

public sealed record ManifestArchivedEventSchema(
    Guid AggregateId,
    DateTimeOffset ArchivedAt);
