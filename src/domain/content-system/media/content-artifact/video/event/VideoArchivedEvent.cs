using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Video;

public sealed record VideoArchivedEvent(
    VideoId VideoId,
    Timestamp ArchivedAt) : DomainEvent;
