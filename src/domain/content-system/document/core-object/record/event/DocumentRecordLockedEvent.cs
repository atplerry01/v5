using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Record;

public sealed record DocumentRecordLockedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentRecordId RecordId,
    string Reason,
    Timestamp LockedAt) : DomainEvent;
