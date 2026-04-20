using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.File;

public sealed record DocumentFileArchivedEvent(
    DocumentFileId DocumentFileId,
    Timestamp ArchivedAt) : DomainEvent;
