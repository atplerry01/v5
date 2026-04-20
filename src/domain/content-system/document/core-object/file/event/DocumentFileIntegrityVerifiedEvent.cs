using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.File;

public sealed record DocumentFileIntegrityVerifiedEvent(
    DocumentFileId DocumentFileId,
    DocumentFileChecksum VerifiedChecksum,
    Timestamp VerifiedAt) : DomainEvent;
