using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Record;

public sealed record DocumentRecordCreatedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentRecordId RecordId,
    DocumentRef DocumentRef,
    Timestamp CreatedAt) : DomainEvent;
