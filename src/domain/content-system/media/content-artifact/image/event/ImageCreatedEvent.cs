using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Image;

public sealed record ImageCreatedEvent(
    ImageId ImageId,
    MediaAssetRef AssetRef,
    MediaFileRef? FileRef,
    ImageDimensions Dimensions,
    ImageOrientation Orientation,
    Timestamp CreatedAt) : DomainEvent;
