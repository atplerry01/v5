namespace Whycespace.Shared.Contracts.Events.Content.Document.Descriptor.Metadata;

public sealed record DocumentMetadataCreatedEventSchema(
    Guid AggregateId,
    Guid DocumentId,
    DateTimeOffset CreatedAt);

public sealed record DocumentMetadataEntryAddedEventSchema(
    Guid AggregateId,
    string Key,
    string Value,
    DateTimeOffset AddedAt);

public sealed record DocumentMetadataEntryUpdatedEventSchema(
    Guid AggregateId,
    string Key,
    string PreviousValue,
    string NewValue,
    DateTimeOffset UpdatedAt);

public sealed record DocumentMetadataEntryRemovedEventSchema(
    Guid AggregateId,
    string Key,
    DateTimeOffset RemovedAt);

public sealed record DocumentMetadataFinalizedEventSchema(
    Guid AggregateId,
    DateTimeOffset FinalizedAt);
