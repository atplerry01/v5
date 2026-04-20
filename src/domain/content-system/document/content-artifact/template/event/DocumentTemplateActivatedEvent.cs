using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Template;

public sealed record DocumentTemplateActivatedEvent(
    DocumentTemplateId TemplateId,
    Timestamp ActivatedAt) : DomainEvent;
