using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.File;

public sealed record DocumentFileIntegrityVerifiedEvent(
    DocumentFileId DocumentFileId,
    DocumentFileChecksum VerifiedChecksum,
    Timestamp VerifiedAt) : DomainEvent;
