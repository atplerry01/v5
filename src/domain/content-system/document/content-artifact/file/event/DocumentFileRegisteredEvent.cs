using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.File;

public sealed record DocumentFileRegisteredEvent(
    DocumentFileId DocumentFileId,
    DocumentRef DocumentRef,
    DocumentFileStorageRef StorageRef,
    DocumentFileChecksum DeclaredChecksum,
    DocumentFileMimeType MimeType,
    DocumentFileSize Size,
    Timestamp RegisteredAt) : DomainEvent;
