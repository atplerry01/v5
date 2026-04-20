using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.File;

public sealed record DocumentFileSupersededEvent(
    DocumentFileId DocumentFileId,
    DocumentFileId SuccessorFileId,
    Timestamp SupersededAt) : DomainEvent;
