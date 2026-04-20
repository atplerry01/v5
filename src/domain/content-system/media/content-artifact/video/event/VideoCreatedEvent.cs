using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Video;

public sealed record VideoCreatedEvent(
    VideoId VideoId,
    MediaAssetRef AssetRef,
    MediaFileRef? FileRef,
    VideoDimensions Dimensions,
    VideoDuration Duration,
    FrameRate FrameRate,
    Timestamp CreatedAt) : DomainEvent;
