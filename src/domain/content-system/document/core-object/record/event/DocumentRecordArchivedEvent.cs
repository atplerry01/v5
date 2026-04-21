using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Record;

public sealed record DocumentRecordArchivedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentRecordId RecordId,
    Timestamp ArchivedAt) : DomainEvent;
