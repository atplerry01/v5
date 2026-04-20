using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.MediaFile;

public sealed record MediaFileSupersededEvent(
    MediaFileId MediaFileId,
    MediaFileId SuccessorFileId,
    Timestamp SupersededAt) : DomainEvent;
