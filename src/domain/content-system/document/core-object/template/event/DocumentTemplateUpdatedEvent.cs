using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Template;

public sealed record DocumentTemplateUpdatedEvent(
    DocumentTemplateId TemplateId,
    TemplateName PreviousName,
    TemplateName NewName,
    TemplateType PreviousType,
    TemplateType NewType,
    TemplateSchemaRef? NewSchemaRef,
    Timestamp UpdatedAt) : DomainEvent;
