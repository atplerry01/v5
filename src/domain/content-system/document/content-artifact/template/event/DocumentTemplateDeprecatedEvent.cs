using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Template;

public sealed record DocumentTemplateDeprecatedEvent(
    DocumentTemplateId TemplateId,
    string Reason,
    Timestamp DeprecatedAt) : DomainEvent;
