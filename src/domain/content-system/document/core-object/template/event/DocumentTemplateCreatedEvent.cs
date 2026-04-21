using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Template;

public sealed record DocumentTemplateCreatedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentTemplateId TemplateId,
    TemplateName Name,
    TemplateType Type,
    TemplateSchemaRef? SchemaRef,
    Timestamp CreatedAt) : DomainEvent;
