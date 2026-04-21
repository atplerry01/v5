using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Document;

public sealed record DocumentMetadataUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentId DocumentId,
    DocumentTitle PreviousTitle,
    DocumentTitle NewTitle,
    DocumentType PreviousType,
    DocumentType NewType,
    DocumentClassification PreviousClassification,
    DocumentClassification NewClassification,
    Timestamp UpdatedAt) : DomainEvent;
