using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Document;

public sealed record DocumentCreatedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentId DocumentId,
    DocumentTitle Title,
    DocumentType Type,
    DocumentClassification Classification,
    StructuralOwnerRef StructuralOwner,
    BusinessOwnerRef BusinessOwner,
    Timestamp CreatedAt) : DomainEvent;
