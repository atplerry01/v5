using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Template;

public sealed record DocumentTemplateArchivedEvent(
    DocumentTemplateId TemplateId,
    Timestamp ArchivedAt) : DomainEvent;
