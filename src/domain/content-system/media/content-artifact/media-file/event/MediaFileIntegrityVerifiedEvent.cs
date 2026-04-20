using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.MediaFile;

public sealed record MediaFileIntegrityVerifiedEvent(
    MediaFileId MediaFileId,
    MediaChecksum VerifiedChecksum,
    Timestamp VerifiedAt) : DomainEvent;
