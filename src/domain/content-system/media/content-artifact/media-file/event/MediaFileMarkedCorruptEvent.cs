using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.MediaFile;

public sealed record MediaFileMarkedCorruptEvent(
    MediaFileId MediaFileId,
    string Reason,
    Timestamp MarkedAt) : DomainEvent;
