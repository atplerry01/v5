using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Document;

public sealed record DocumentActivatedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentId DocumentId,
    Timestamp ActivatedAt) : DomainEvent;
