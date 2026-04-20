using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Template;

public sealed record DocumentTemplateCreatedEvent(
    DocumentTemplateId TemplateId,
    TemplateName Name,
    TemplateType Type,
    TemplateSchemaRef? SchemaRef,
    Timestamp CreatedAt) : DomainEvent;
