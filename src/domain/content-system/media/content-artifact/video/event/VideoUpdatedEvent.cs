using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Video;

public sealed record VideoUpdatedEvent(
    VideoId VideoId,
    VideoDimensions Dimensions,
    VideoDuration Duration,
    FrameRate FrameRate,
    Timestamp UpdatedAt) : DomainEvent;
