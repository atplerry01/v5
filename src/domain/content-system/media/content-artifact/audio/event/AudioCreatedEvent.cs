using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Audio;

public sealed record AudioCreatedEvent(
    AudioId AudioId,
    MediaAssetRef AssetRef,
    MediaFileRef? FileRef,
    AudioFormat Format,
    AudioDuration Duration,
    ChannelCount Channels,
    Timestamp CreatedAt) : DomainEvent;
