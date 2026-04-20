using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Document;

public sealed record DocumentActivatedEvent(
    DocumentId DocumentId,
    Timestamp ActivatedAt) : DomainEvent;
