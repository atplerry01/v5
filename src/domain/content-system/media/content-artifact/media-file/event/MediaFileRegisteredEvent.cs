using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.MediaFile;

public sealed record MediaFileRegisteredEvent(
    MediaFileId MediaFileId,
    StorageReference StorageReference,
    MediaChecksum DeclaredChecksum,
    MediaMimeType MimeType,
    FileSize Size,
    Timestamp RegisteredAt) : DomainEvent;
