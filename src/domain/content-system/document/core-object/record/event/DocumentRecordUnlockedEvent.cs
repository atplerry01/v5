using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Record;

public sealed record DocumentRecordUnlockedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentRecordId RecordId,
    Timestamp UnlockedAt) : DomainEvent;
