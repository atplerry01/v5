using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Template;

public sealed record DocumentTemplateArchivedEvent(
    DocumentTemplateId TemplateId,
    Timestamp ArchivedAt) : DomainEvent;
