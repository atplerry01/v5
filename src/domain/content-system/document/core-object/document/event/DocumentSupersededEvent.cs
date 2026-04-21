using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Document;

public sealed record DocumentSupersededEvent(
    [property: JsonPropertyName("AggregateId")] DocumentId DocumentId,
    DocumentId SupersedingDocumentId,
    Timestamp SupersededAt) : DomainEvent;
