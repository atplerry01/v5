using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Document;

public sealed record DocumentMetadataUpdatedEvent(
    DocumentId DocumentId,
    DocumentTitle PreviousTitle,
    DocumentTitle NewTitle,
    DocumentType PreviousType,
    DocumentType NewType,
    DocumentClassification PreviousClassification,
    DocumentClassification NewClassification,
    Timestamp UpdatedAt) : DomainEvent;
