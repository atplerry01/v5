using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Template;

public sealed record DocumentTemplateActivatedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentTemplateId TemplateId,
    Timestamp ActivatedAt) : DomainEvent;
