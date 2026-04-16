namespace Whycespace.Shared.Contracts.Events.Content.Media.Asset;

public sealed record MediaAssetRegisteredEventSchema(
    Guid AggregateId,
    Guid MediaAssetId,
    string OwnerRef,
    int MediaType,
    string Title,
    string Description,
    string ContentDigest,
    string StorageUri,
    long StorageSizeBytes,
    DateTimeOffset RegisteredAt);

public sealed record MediaAssetProcessingStartedEventSchema(
    Guid AggregateId,
    Guid MediaAssetId,
    DateTimeOffset StartedAt);

public sealed record MediaAssetAvailableEventSchema(
    Guid AggregateId,
    Guid MediaAssetId,
    DateTimeOffset AvailableAt);

public sealed record MediaAssetPublishedEventSchema(
    Guid AggregateId,
    Guid MediaAssetId,
    DateTimeOffset PublishedAt);

public sealed record MediaAssetUnpublishedEventSchema(
    Guid AggregateId,
    Guid MediaAssetId,
    DateTimeOffset UnpublishedAt);

public sealed record MediaAssetArchivedEventSchema(
    Guid AggregateId,
    Guid MediaAssetId,
    DateTimeOffset ArchivedAt);

public sealed record MediaAssetMetadataUpdatedEventSchema(
    Guid AggregateId,
    Guid MediaAssetId,
    string Title,
    string Description,
    IReadOnlyList<string> Tags,
    DateTimeOffset UpdatedAt);
