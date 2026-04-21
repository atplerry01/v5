using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Record;

public sealed record DocumentRecordClosedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentRecordId RecordId,
    RecordClosureReason Reason,
    Timestamp ClosedAt) : DomainEvent;
