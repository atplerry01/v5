using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.ContentArtifact.Template;

public readonly record struct DocumentTemplateId
{
    public Guid Value { get; }

    public DocumentTemplateId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "DocumentTemplateId cannot be empty.");
        Value = value;
    }
}
