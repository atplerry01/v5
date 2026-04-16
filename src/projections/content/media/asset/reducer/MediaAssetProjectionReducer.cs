using Whycespace.Shared.Contracts.Content.Media.Asset;
using Whycespace.Shared.Contracts.Events.Content.Media.Asset;

namespace Whycespace.Projections.Content.Media.Asset.Reducer;

/// <summary>
/// Pure state reducer for the MediaAsset read model. No I/O, no side effects.
/// </summary>
public static class MediaAssetProjectionReducer
{
    public static MediaAssetReadModel Apply(MediaAssetReadModel state, MediaAssetRegisteredEventSchema e)
        => state with
        {
            OwnerRef = e.OwnerRef,
            MediaType = e.MediaType,
            Title = e.Title,
            Description = e.Description,
            ContentDigest = e.ContentDigest,
            StorageUri = e.StorageUri,
            StorageSizeBytes = e.StorageSizeBytes,
            Status = "draft",
            RegisteredAt = e.RegisteredAt
        };

    public static MediaAssetReadModel Apply(MediaAssetReadModel state, MediaAssetProcessingStartedEventSchema e)
        => state with { Status = "processing", LastTransitionedAt = e.StartedAt };

    public static MediaAssetReadModel Apply(MediaAssetReadModel state, MediaAssetAvailableEventSchema e)
        => state with { Status = "available", LastTransitionedAt = e.AvailableAt };

    public static MediaAssetReadModel Apply(MediaAssetReadModel state, MediaAssetPublishedEventSchema e)
        => state with { Status = "published", LastTransitionedAt = e.PublishedAt };

    public static MediaAssetReadModel Apply(MediaAssetReadModel state, MediaAssetUnpublishedEventSchema e)
        => state with { Status = "available", LastTransitionedAt = e.UnpublishedAt };

    public static MediaAssetReadModel Apply(MediaAssetReadModel state, MediaAssetArchivedEventSchema e)
        => state with { Status = "archived", LastTransitionedAt = e.ArchivedAt };

    public static MediaAssetReadModel Apply(MediaAssetReadModel state, MediaAssetMetadataUpdatedEventSchema e)
        => state with
        {
            Title = e.Title,
            Description = e.Description,
            Tags = e.Tags,
            LastTransitionedAt = e.UpdatedAt
        };
}
