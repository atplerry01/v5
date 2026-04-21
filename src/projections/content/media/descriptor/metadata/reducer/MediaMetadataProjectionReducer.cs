using Whycespace.Shared.Contracts.Content.Media.Descriptor.Metadata;
using Whycespace.Shared.Contracts.Events.Content.Media.Descriptor.Metadata;

namespace Whycespace.Projections.Content.Media.Descriptor.Metadata.Reducer;

public static class MediaMetadataProjectionReducer
{
    public static MediaMetadataReadModel Apply(MediaMetadataReadModel state, MediaMetadataCreatedEventSchema e) =>
        state with
        {
            MetadataId = e.AggregateId,
            AssetRef = e.AssetRef,
            Status = "Open",
            CreatedAt = e.CreatedAt,
            LastModifiedAt = e.CreatedAt
        };

    public static MediaMetadataReadModel Apply(MediaMetadataReadModel state, MediaMetadataEntryAddedEventSchema e) =>
        state with { MetadataId = e.AggregateId, LastModifiedAt = e.AddedAt };

    public static MediaMetadataReadModel Apply(MediaMetadataReadModel state, MediaMetadataEntryUpdatedEventSchema e) =>
        state with { MetadataId = e.AggregateId, LastModifiedAt = e.UpdatedAt };

    public static MediaMetadataReadModel Apply(MediaMetadataReadModel state, MediaMetadataEntryRemovedEventSchema e) =>
        state with { MetadataId = e.AggregateId, LastModifiedAt = e.RemovedAt };

    public static MediaMetadataReadModel Apply(MediaMetadataReadModel state, MediaMetadataFinalizedEventSchema e) =>
        state with { MetadataId = e.AggregateId, Status = "Finalized", FinalizedAt = e.FinalizedAt, LastModifiedAt = e.FinalizedAt };
}
