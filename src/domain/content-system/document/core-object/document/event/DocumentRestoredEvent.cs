using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Document;

public sealed record DocumentRestoredEvent(
    [property: JsonPropertyName("AggregateId")] DocumentId DocumentId,
    Timestamp RestoredAt) : DomainEvent;
