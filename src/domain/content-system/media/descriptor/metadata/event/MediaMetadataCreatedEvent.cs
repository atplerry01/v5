using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;

public sealed record MediaMetadataCreatedEvent(
    MediaMetadataId MetadataId,
    MediaAssetRef AssetRef,
    Timestamp CreatedAt) : DomainEvent;
