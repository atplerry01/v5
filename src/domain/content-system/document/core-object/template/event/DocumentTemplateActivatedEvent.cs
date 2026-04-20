using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Template;

public sealed record DocumentTemplateActivatedEvent(
    DocumentTemplateId TemplateId,
    Timestamp ActivatedAt) : DomainEvent;
