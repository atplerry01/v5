using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Document;

public sealed record DocumentCreatedEvent(
    DocumentId DocumentId,
    DocumentTitle Title,
    DocumentType Type,
    DocumentClassification Classification,
    Timestamp CreatedAt) : DomainEvent;
