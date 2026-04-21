namespace Whycespace.Shared.Contracts.Events.Content.Media.Descriptor.Metadata;

public sealed record MediaMetadataCreatedEventSchema(
    Guid AggregateId,
    Guid AssetRef,
    DateTimeOffset CreatedAt);

public sealed record MediaMetadataEntryAddedEventSchema(
    Guid AggregateId,
    string Key,
    string Value,
    DateTimeOffset AddedAt);

public sealed record MediaMetadataEntryUpdatedEventSchema(
    Guid AggregateId,
    string Key,
    string PreviousValue,
    string NewValue,
    DateTimeOffset UpdatedAt);

public sealed record MediaMetadataEntryRemovedEventSchema(
    Guid AggregateId,
    string Key,
    DateTimeOffset RemovedAt);

public sealed record MediaMetadataFinalizedEventSchema(
    Guid AggregateId,
    DateTimeOffset FinalizedAt);
