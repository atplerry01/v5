using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Video;

public sealed record VideoActivatedEvent(
    VideoId VideoId,
    Timestamp ActivatedAt) : DomainEvent;
